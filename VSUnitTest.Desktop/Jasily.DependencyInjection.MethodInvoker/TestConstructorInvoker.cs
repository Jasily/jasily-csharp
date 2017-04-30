﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using Jasily.DependencyInjection.MethodInvoker;
using System.Linq;

namespace VSUnitTest.Desktop.Jasily.DependencyInjection.MethodInvoker
{
    [TestClass]
    public class TestConstructorInvoker
    {
        public class Class1
        {
            public Class1(string key)
            {
                Assert.AreEqual("24", key);
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var sc = new ServiceCollection();
            sc.UseMethodInvoker();
            var provider = sc.BuildServiceProvider();
            var factory = provider.GetService<IMethodInvokerFactory<Class1>>();
            Assert.IsNotNull(factory);

            var args = new OverrideArguments();
            args.AddArgument("key", "24");

            var constructor = typeof(Class1).GetConstructors().Single();
            Assert.IsInstanceOfType(factory.InvokeConstructor(constructor, provider, args), typeof(Class1));
        }

        public class Class2
        {
            public Class2(string key)
            {
                Assert.AreEqual("24", key);
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            var sc = new ServiceCollection();
            sc.UseMethodInvoker();
            sc.AddSingleton("sc");
            var provider = sc.BuildServiceProvider();
            var factory = provider.GetService<IMethodInvokerFactory<Class2>>();
            Assert.IsNotNull(factory);

            var args = new OverrideArguments();
            args.AddArgument("key", "24");

            var constructor = typeof(Class2).GetConstructors().Single();
            Assert.IsInstanceOfType(factory.InvokeConstructor(constructor, provider, args), typeof(Class2));
        }

        public class Class3
        {
            public Class3(string key)
            {
                Assert.AreEqual("24", key);
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            var sc = new ServiceCollection();
            sc.UseMethodInvoker();
            sc.AddSingleton("24");
            var provider = sc.BuildServiceProvider();
            var factory = provider.GetService<IMethodInvokerFactory<Class3>>();
            Assert.IsNotNull(factory);

            var constructor = typeof(Class3).GetConstructors().Single();
            Assert.IsInstanceOfType(factory.InvokeConstructor(constructor, provider), typeof(Class3));
        }
    }
}
