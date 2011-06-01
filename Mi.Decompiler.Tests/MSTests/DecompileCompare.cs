using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mi.Decompiler.Tests;
using System.Diagnostics;

namespace MSTests
{
    [TestClass]
    public class DecompileCompare
    {
        [TestMethod]
        public void Test()
        {
            bool failedAny = false;
            foreach (var item in TestingLogic.GetTests("..\\..\\..\\"))
            {
                try
                {
                    item.Value();
                }
                catch (Exception error)
                {
                    Debug.WriteLine(item.Key + " " + error.ToString());
                }

                failedAny = true;
            }

            Assert.IsFalse(failedAny);
        }
    }
}
