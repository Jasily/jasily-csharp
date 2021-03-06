using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasily.DependencyInjection.MethodInvoker;
using Jasily.Extensions.System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Jasily.DependencyInjection.AwaiterAdapter.Internal
{
    internal abstract class AwaitableAdapter : IAwaitableAdapter
    {
        private readonly bool _isValueType;

        protected AwaitableAdapter(AwaitableInfo info, IServiceProvider serviceProvider)
        {
            this.AwaitableInfo = info ?? throw new ArgumentNullException(nameof(info));
            this.ServiceProvider = serviceProvider;
            this._isValueType = info.AwaitableType.GetTypeInfo().IsValueType;
        }

        protected IServiceProvider ServiceProvider { get; }

        protected AwaitableInfo AwaitableInfo { get; }

        public bool IsAwaitable => true;

        public abstract bool IsCompleted(object instance);

        public abstract object GetResult(object instance);

        public abstract void OnCompleted(object instance, Action continuation);

        protected void VerifyArgument<T>([NotNull] T instance)
        {
            if (!this._isValueType && instance == null) throw new ArgumentNullException(nameof(instance));
            if (!this.IsAwaitable) throw new InvalidOperationException("is NOT awaitable.");
        }
    }

    internal abstract class AwaitableAdapter<TInstance, TAwaiter> : AwaitableAdapter
    {
        private readonly IInstanceMethodInvoker<TInstance, TAwaiter> _getAwaiterInvoker;

        protected AwaitableAdapter(AwaitableInfo info, IServiceProvider serviceProvider)
            : base(info, serviceProvider)
        {
            this._getAwaiterInvoker = this.ServiceProvider
                .GetRequiredService<IMethodInvokerFactory<TInstance>>()
                .GetInstanceMethodInvoker(this.AwaitableInfo.GetAwaiterMethod)
                .HasResult<TInstance, TAwaiter>();
        }

        protected abstract AwaiterAdapter<TAwaiter> AwaiterAdapter { get; }

        protected TAwaiter GetAwaiter(TInstance instance)
        {
            this.VerifyArgument(instance);
            return this._getAwaiterInvoker.Invoke(instance, this.ServiceProvider);
        }

        public bool IsCompleted(TInstance instance) => this.AwaiterAdapter.IsCompleted(this.GetAwaiter(instance));

        public void OnCompleted(TInstance instance, Action continuation)
        {
            this.AwaiterAdapter.OnCompleted(this.GetAwaiter(instance), continuation);
        }

        public override bool IsCompleted(object instance) => this.IsCompleted((TInstance)instance);

        public override void OnCompleted(object instance, Action continuation) => this.OnCompleted((TInstance)instance, continuation);
    }
}