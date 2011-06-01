using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

#if SILVERLIGHT
using Microsoft.Silverlight.Testing;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mi.Decompiler.Tests
{
    [TestClass]
    public class Empty
    {
        [TestMethod]
        public void IntArray() { Array<int>(); }

        [TestMethod]
        public void DoubleArray() { Array<double>(); }

        [TestMethod]
        public void ObjectArray() { Array<object>(); }

        [TestMethod]
        public void IntReadOnlyCollection() { ReadOnlyCollection<int>(); }

        [TestMethod]
        public void DoubleReadOnlyCollection() { ReadOnlyCollection<double>(); }

        [TestMethod]
        public void ObjectReadOnlyCollection() { ReadOnlyCollection<object>(); }

        static void Array<T>()
        {
            Assert.IsTrue(Mi.Empty.Array<T>().Length == 0);
        }

        static void ReadOnlyCollection<T>()
        {
            Assert.IsTrue(Mi.Empty.ReadOnlyCollection<T>().Count == 0);
        }
    }
}