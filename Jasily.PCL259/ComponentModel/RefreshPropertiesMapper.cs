﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Jasily.ComponentModel
{
    public class RefreshPropertiesMapper
    {
        private readonly Type type;
        private readonly PropertyChangedEventArgs[] properties;

        private RefreshPropertiesMapper([NotNull] Type type)
        {
            this.type = type;
            this.properties = MapNotifyPropertyChangedAttribute(type);
        }

        internal PropertyChangedEventArgs[] GetProperties(NotifyPropertyChangedObject obj)
        {
            if (obj.GetType() != this.type) throw new InvalidOperationException();

            return this.properties;
        }

        internal static PropertyChangedEventArgs[] MapNotifyPropertyChangedAttribute(Type type)
        {
            return (
                from property in type.GetRuntimeProperties()
                let attr = property.GetCustomAttribute<NotifyPropertyChangedAttribute>()
                where attr != null
                orderby attr.Order
                select new PropertyChangedEventArgs(property.Name)
                ).ToArray();
        }

        public static RefreshPropertiesMapper OfType<T>() => InstanceStore<T>.Instance;

        private static class InstanceStore<T>
        {
            public static RefreshPropertiesMapper Instance { get; } = new RefreshPropertiesMapper(typeof(T));
        }
    }
}