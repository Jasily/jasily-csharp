using System;

namespace Jasily.ComponentModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotifyPropertyChangedAttribute : Attribute
    {
        /// <summary>
        /// order by asc
        /// </summary>
        public int Order { get; set; }
    }
}