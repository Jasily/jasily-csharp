﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Jasily.Core;

namespace VSUnitTest.Desktop.Jasily.Extensions.System.Linq
{
    [TestClass]
    public class TestEnumerableExtensions
    {
        [TestMethod]
        public void TestAppend()
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }.Append(4).ToArray(), new[] { 1, 2, 3, 4 });
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }.Append(4, Position.Start).ToArray(), new[] { 4, 1, 2, 3 });

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }.Append(4, 0).ToArray(), new[] { 4, 1, 2, 3 });
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }.Append(4, 1).ToArray(), new[] { 1, 4, 2, 3 });
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }.Append(4, 3).ToArray(), new[] { 1, 2, 3, 4 });
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }.Append(4, 4).ToArray(), new[] { 1, 2, 3, 4 });
        }
    }
}