using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mi.Decompiler.Tests.CustomAttributes
{
	[TestClass]
	public class CustomAttributeTests : DecompilerTestBase
	{
		[TestMethod]
		public void CustomAttributeSamples()
		{
			ValidateFileRoundtrip(@"S_CustomAttributeSamples");
		}

		[TestMethod]
		public void CustomAttributesMultiTest()
		{
			ValidateFileRoundtrip(@"S_CustomAttributes");
		}

		[TestMethod]
		public void AssemblyCustomAttributesMultiTest()
		{
			ValidateFileRoundtrip(@"S_AssemblyCustomAttribute");
		}
	}
}
