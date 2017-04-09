﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Jasily.DependencyInjection.Internal.CallSites;

namespace Jasily.DependencyInjection.Internal
{
    internal interface IServiceResolver : IDisposable
    {
        ServiceProvider ServiceProvider { get; }

        ServiceEntry ResolveServiceEntry(ResolveRequest request, ResolveLevel level);

        ResolveResult ResolveValue(ServiceProvider provider, ResolveLevel level, ResolveRequest request);
    }

    internal class ServiceResolver : IServiceResolver
    {
        private readonly List<Service> services = new List<Service>();
        private readonly Dictionary<Type, TypedServiceEntry> typedServices;
        private readonly Dictionary<string, NamedServiceEntry> namedServices;
        private readonly ConcurrentDictionary<ResolveRequest, ServiceBuilder> builders;

        /// <summary>
        /// The ServiceResolver owner.
        /// </summary>
        public ServiceProvider ServiceProvider { get; }

        public ServiceResolver(ServiceProvider provider, ServiceProviderSettings settings,
            IEnumerable<NamedServiceDescriptor> serviceDescriptors = null)
        {
            this.ServiceProvider = provider;

            if (serviceDescriptors == null)
            {
                this.typedServices = new Dictionary<Type, TypedServiceEntry>();
                this.namedServices = new Dictionary<string, NamedServiceEntry>(settings.NameComparer);
            }
            else
            {
                var services = serviceDescriptors.Select(z => new Service(this, z)).ToArray();
                this.typedServices = this.CreateTypedServiceDictionary(services, settings.NameComparer);
                this.namedServices = this.CreateNamedServiceDictionary(services, settings.NameComparer);
            }
        }

        [CanBeNull]
        public ServiceEntry ResolveServiceEntry(ResolveRequest request, ResolveLevel level)
        {
            switch (level)
            {
                case ResolveLevel.TypeAndName:
                case ResolveLevel.Type:
                    if (this.typedServices.TryGetValue(request.ServiceType, out var o))
                        return o;
                    break;

                case ResolveLevel.NameAndType:
                    if (request.ServiceName != string.Empty)
                    {
                        if (this.namedServices.TryGetValue(request.ServiceName, out var e))
                            return e;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private Service ResolveService(ServiceProvider provider, ResolveLevel level, ResolveRequest request)
        {
            var serviceEntry = this.ResolveServiceEntry(request, level);
            return serviceEntry?.Resolve(request, level);
        }

        private IServiceCallSite ResolveCallSite(ServiceProvider provider, ResolveLevel level, ResolveRequest request,
            ISet<Service> serviceChain)
        {
            var service = this.ResolveService(provider, level, request);
            if (service == null) return null;

            try
            {
                if (!serviceChain.Add(service)) throw new InvalidOperationException();
                return service.GetCallSite(provider, serviceChain);
            }
            finally
            {
                serviceChain.Remove(service);
            }
        }

        public ResolveResult ResolveValue(ServiceProvider provider, ResolveLevel level, ResolveRequest request)
        {
            var serviceEntry = this.ResolveServiceEntry(request, level);
            var service = serviceEntry?.Resolve(request, level);
            if (service == null) return default(ResolveResult);
            provider = service.Descriptor.Lifetime == ServiceLifetime.Singleton
                ? provider.RootProvider
                : provider;
            var value = service.GetValue(provider);
            return new ResolveResult(value);
        }

        public void Dispose()
        {
            foreach (var service in this.services)
            {
                service.Dispose();
            }
            this.services.Clear();
            this.typedServices.Clear();
            this.namedServices.Clear();
        }
    }

    internal class ConcurrentServiceResolver : IServiceResolver
    {
        private readonly ConcurrentDictionary<Type, TypedServiceEntry> typedServices;
        private readonly ConcurrentDictionary<string, NamedServiceEntry> namedServices;
        private readonly ConcurrentDictionary<ResolveRequest, ServiceBuilder> builders;

        /// <summary>
        /// The ServiceResolver owner.
        /// </summary>
        public ServiceProvider ServiceProvider { get; }

        public ConcurrentServiceResolver(ServiceProvider provider, ServiceProviderSettings settings,
            [NotNull] IEnumerable<NamedServiceDescriptor> serviceDescriptors)
        {
            this.ServiceProvider = provider;

            if (serviceDescriptors == null)
            {
                this.typedServices = new ConcurrentDictionary<Type, TypedServiceEntry>();
                this.namedServices = new ConcurrentDictionary<string, NamedServiceEntry>(settings.NameComparer);
            }
            else
            {
                var services = serviceDescriptors.Select(z => new Service(this, z)).ToArray();
                this.typedServices = new ConcurrentDictionary<Type, TypedServiceEntry>(
                    this.CreateTypedServiceDictionary(services, settings.NameComparer));
                this.namedServices = new ConcurrentDictionary<string, NamedServiceEntry>(
                    this.CreateNamedServiceDictionary(services, settings.NameComparer), settings.NameComparer);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ServiceEntry ResolveServiceEntry(ResolveRequest request, ResolveLevel level)
        {
            throw new NotImplementedException();
        }

        public ResolveResult ResolveValue(ServiceProvider provider, ResolveLevel level, ResolveRequest request)
        {
            var serviceEntry = this.ResolveServiceEntry(request, level);
            var service = serviceEntry?.Resolve(request, level);
            if (service == null) return default(ResolveResult);
            provider = service.Descriptor.Lifetime == ServiceLifetime.Singleton
                ? provider.RootProvider
                : provider;
            var value = service.GetValue(provider);
            return new ResolveResult(value);
        }
    }

    internal static class ServiceResolverHelper
    {
        public static Dictionary<Type, TypedServiceEntry> CreateTypedServiceDictionary(
            this IServiceResolver resolver, [NotNull] Service[] services, [NotNull] StringComparer comparer)
        {
            Debug.Assert(services != null && comparer != null);
            return services.GroupBy(z => z.ServiceType)
                .Select(li => li.Aggregate(new TypedServiceEntry(li.Key, comparer),(e, i) => { e.Add(i); return e; }))
                .ToDictionary(z => z.ServiceType, z => z);
        }

        public static Dictionary<string, NamedServiceEntry> CreateNamedServiceDictionary(
            this IServiceResolver resolver, [NotNull] Service[] services, [NotNull] StringComparer comparer)
        {
            Debug.Assert(services != null && comparer != null);
            return services.GroupBy(z => z.ServiceName, comparer)
                .Select(li => li.Aggregate(new NamedServiceEntry(li.Key), (e, i) => { e.Add(i); return e; }))
                .ToDictionary(z => z.ServiceName, z => z, comparer);
        }

        public static ResolveResult ResolveValue(this IServiceResolver resolver,
            ServiceProvider provider, ResolveRequest request)
        {
            for (var i = 0; i < provider.RootProvider.ResolveMode.Length; i++)
            {
                var level = provider.RootProvider.ResolveMode[i];
                var result = resolver.ResolveValue(provider, request);
                if (result.HasValue) return result;
            }
            return default(ResolveResult);
        }
    }
}