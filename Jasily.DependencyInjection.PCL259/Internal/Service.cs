using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Jasily.DependencyInjection.Internal
{
    internal class Service : IValueStore, IDisposable
    {
        private static readonly Func<ServiceProvider, object> Null = (_ => null);

        private IValueStore valueStore;
        private bool isValueCreated;
        private object value;
        private Func<ServiceProvider, object> instanceAccessor;
        private TypeInfo serviceTypeInfo;

        public Service(IServiceDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }

        public IServiceDescriptor Descriptor { get; }

        [NotNull]
        public string ServiceName => this.Descriptor.ServiceName ?? string.Empty;

        [NotNull]
        public Type ServiceType => this.Descriptor.ServiceType;

        [NotNull]
        public TypeInfo ServiceTypeInfo
            => this.serviceTypeInfo ?? (this.serviceTypeInfo = this.ServiceType.GetTypeInfo());

        object IValueStore.GetValue(Service service, ServiceProvider provider, Func<ServiceProvider, object> creator)
        {
            if (this.isValueCreated) return this.value;

            lock (this)
            {
                this.value = creator(provider);
                this.isValueCreated = true;
            }

            return this.value;
        }

        public object GetValue(ServiceProvider provider)
        {
            if (this.instanceAccessor == null)
            {
                Interlocked.CompareExchange(ref this.instanceAccessor, this.CreateServiceAccessor(provider), null);
            }

            if (this.Descriptor.Lifetime != ServiceLifetime.Transient)
            {
                if (this.valueStore == null)
                {
                    var store = this.Descriptor.Lifetime == ServiceLifetime.Singleton
                        ? this
                        : (IValueStore) provider;
                    Interlocked.CompareExchange(ref this.valueStore, store, null);
                }

                return this.valueStore.GetValue(this, provider, this.instanceAccessor);
            }
            else
            {
                return this.instanceAccessor(provider);
            }
        }

        public IServiceCallSite GetCallSite(ServiceProvider provider, ISet<IServiceDescriptor> serviceChain)
        {
            var callSite = (this.Descriptor as IServiceCallSite) ??
                           (this.Descriptor as IServiceCallSiteProvider)?.CreateServiceCallSite(provider, serviceChain);
            if (callSite != null) return callSite;

            throw new NotImplementedException();
        }

        private Func<ServiceProvider, object> CreateServiceAccessor(ServiceProvider serviceProvider)
        {
            var callSite = this.GetCallSite(serviceProvider, new HashSet<IServiceDescriptor>());
            if (callSite == null) return Null;
            if (callSite is IImmutableCallSite) return callSite.ResolveValue;
            return RealizeServiceAccessor(serviceProvider.RootProvider, this, callSite);
        }

        private static Func<ServiceProvider, object> RealizeServiceAccessor(RootServiceProvider serviceProvider,
            Service service, IServiceCallSite callSite)
        {
            Debug.Assert(serviceProvider.Setting.CompileAfterCallCount != null);
            var compileAfter = serviceProvider.Setting.CompileAfterCallCount.Value;

            var callCount = 0;
            return provider =>
            {
                if (Interlocked.Increment(ref callCount) == compileAfter)
                {
                    Task.Run(() =>
                    {
                        serviceProvider.Log($"begin compile {service}");
                        var func = new CallSiteExpressionBuilder().Build(callSite);
                        Interlocked.Exchange(ref service.instanceAccessor, func);
                    });
                }
                return callSite.ResolveValue(provider);
            };
        }

        public override string ToString() => $"{this.ServiceType.Name} {this.ServiceName}";

        public void Dispose()
        {
            this.isValueCreated = false;
            (this.valueStore as IDisposable)?.Dispose();
            this.valueStore = null;
        }
    }
}