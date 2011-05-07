using System;

using Mi.Assemblies;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mi.Assemblies.Tests {

	[TestClass]
	public class AssemblyTests : BaseTestFixture {

		[TestModule ("hello.exe")]
		public void Name (ModuleDefinition module)
		{
			var name = module.Assembly.Name;

			Assert.IsNotNull (name);

			Assert.AreEqual ("hello", name.Name);
			Assert.AreEqual (string.Empty, name.Culture);
			Assert.AreEqual (new Version (0, 0, 0, 0), name.Version);
			Assert.AreEqual (AssemblyHashAlgorithm.SHA1, name.HashAlgorithm);
		}
	}
}
