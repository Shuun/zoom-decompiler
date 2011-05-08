using System;

using Mi.Assemblies;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mi.Decompiler.Tests;

namespace Mi.Assemblies.Tests {

	[TestClass]
	public class NestedTypesTests 
    {
		[TestMethod]
		public void NestedTypes ()
		{
            var module = SampleInputLoader.LoadAssembly("NestedTypes").MainModule;
			var foo = module.GetType ("Foo");

			Assert.AreEqual ("Foo", foo.Name);
			Assert.AreEqual ("Foo", foo.FullName);
			Assert.AreEqual (module, foo.Module);
			Assert.AreEqual (1, foo.NestedTypes.Count);

			var bar = foo.NestedTypes [0];

			Assert.AreEqual ("Bar", bar.Name);
			Assert.AreEqual ("Foo/Bar", bar.FullName);
			Assert.AreEqual (module, bar.Module);
			Assert.AreEqual (1, bar.NestedTypes.Count);

			var baz = bar.NestedTypes [0];

			Assert.AreEqual ("Baz", baz.Name);
			Assert.AreEqual ("Foo/Bar/Baz", baz.FullName);
			Assert.AreEqual (module, baz.Module);
		}

		[TestMethod]
        public void DirectNestedType()
        {
            var module = SampleInputLoader.LoadAssembly("NestedTypes").MainModule;
			var bingo = module.GetType ("Bingo");
			var get_fuel = bingo.GetMethod ("GetFuel");

			Assert.AreEqual ("Bingo/Fuel", get_fuel.ReturnType.FullName);
		}
	}
}
