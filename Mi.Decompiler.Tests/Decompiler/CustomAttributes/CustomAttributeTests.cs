using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ICSharpCode.Decompiler.Tests.CustomAttributes
{
	[TestClass]
	public class CustomAttributeTests : DecompilerTestBase
	{
		[TestMethod]
		public void CustomAttributeSamples()
		{
			ValidateFileRoundtrip(@"CustomAttributes\S_CustomAttributeSamples.cs");
		}

		[TestMethod]
		public void CustomAttributesMultiTest()
		{
			ValidateFileRoundtrip(@"CustomAttributes\S_CustomAttributes.cs");
		}

		[TestMethod]
		public void AssemblyCustomAttributesMultiTest()
		{
			ValidateFileRoundtrip(@"CustomAttributes\S_AssemblyCustomAttribute.cs");
		}
	}
}
