﻿using System;

namespace Jasily.DependencyInjection.Internal
{
    /// <summary>
    /// Here are the positions how to store:
    /// Singleton: store in <see cref="ServiceBuilder"/>;
    /// Scoped: store in <see cref="ServiceProvider"/>;
    /// Transient: store in <see cref="ServiceProvider"/>;
    /// </summary>
    internal interface IValueStore : IDisposable
    {
        object GetValue(ServiceBuilder builder, ServiceProvider provider, Func<ServiceProvider, object> creator);
    }
}