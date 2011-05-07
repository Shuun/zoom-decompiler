using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mi.Decompiler.Tests.Types
{
	[TestClass]
	public class TypeTests : DecompilerTestBase
	{
		[TestMethod]
		public void TypeMemberDeclarations()
		{
			ValidateFileRoundtrip(@"S_TypeMemberDeclarations");
		}
	}
}
