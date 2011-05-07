using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ICSharpCode.Decompiler.Tests.Types
{
	[TestClass]
	public class EnumTests : DecompilerTestBase
	{
		[TestMethod]
		public void EnumSamples()
		{
			ValidateFileRoundtrip(@"Types\S_EnumSamples.cs");
		}
	}
}
