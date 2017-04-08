﻿using System.Collections.Generic;
using System.Linq;
using Jasily.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;

namespace VSUnitTest.Desktop.Jasily.DependencyInjection
{
    [TestClass]
    public class TestServiceProvider
    {
        [TestMethod]
        public void TestMethod1()
        {
            var sc = new ServiceCollection();
            var obj1 = "5";
            var obj2 = new object();
            sc.AddSingleton(obj1).AssignNameToLast("key");
            sc.AddSingleton(obj2).AssignNameToLast("value");
            sc.AddScopedValue<KeyValuePair<string, object>?>(new KeyValuePair<string, object>(obj1, obj2));
            var provider = sc.BuildServiceProvider(new ServiceProviderSettings { EnableDebug = true });
            foreach (var _ in Enumerable.Range(0, 10))
            {
                var service = provider.GetService(typeof(KeyValuePair<string, object>?));
                Assert.IsNotNull(service);
                Assert.IsInstanceOfType(service, typeof(KeyValuePair<string, object>?));
                var kvp = (KeyValuePair<string, object>)service;
                Assert.AreEqual(obj1, kvp.Key);
                Assert.AreEqual(obj2, kvp.Value);
            }
        }
    }
}
