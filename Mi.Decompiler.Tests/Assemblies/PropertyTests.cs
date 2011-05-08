using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mi.Decompiler.Tests;

namespace Mi.Assemblies.Tests {

	[TestClass]
	public class PropertyTests 
    {
		[TestMethod]
		public void AbstractMethod ()
		{
            var module = SampleInputLoader.LoadAssembly("Properties").MainModule;
			var type = module.GetType ("Foo");

			Assert.IsTrue (type.HasProperties);

			var properties = type.Properties;

			Assert.AreEqual (3, properties.Count);

			var property = properties [0];

			Assert.IsNotNull (property);
			Assert.AreEqual ("Bar", property.Name);
			Assert.IsNotNull (property.PropertyType);
			Assert.AreEqual ("System.Int32", property.PropertyType.FullName);

			Assert.IsNotNull (property.GetMethod);
			Assert.AreEqual (MethodSemanticsAttributes.Getter, property.GetMethod.SemanticsAttributes);
			Assert.IsNull (property.SetMethod);

			property = properties [1];

			Assert.IsNotNull (property);
			Assert.AreEqual ("Baz", property.Name);
			Assert.IsNotNull (property.PropertyType);
			Assert.AreEqual ("System.String", property.PropertyType.FullName);

			Assert.IsNotNull (property.GetMethod);
			Assert.AreEqual (MethodSemanticsAttributes.Getter, property.GetMethod.SemanticsAttributes);
			Assert.IsNotNull (property.SetMethod);
			Assert.AreEqual (MethodSemanticsAttributes.Setter, property.SetMethod.SemanticsAttributes);

			property = properties [2];

			Assert.IsNotNull (property);
			Assert.AreEqual ("Gazonk", property.Name);
			Assert.IsNotNull (property.PropertyType);
			Assert.AreEqual ("System.String", property.PropertyType.FullName);

			Assert.IsNull (property.GetMethod);
			Assert.IsNotNull (property.SetMethod);
			Assert.AreEqual (MethodSemanticsAttributes.Setter, property.SetMethod.SemanticsAttributes);
		}

		[TestMethod]
        public void OtherMethod()
        {
            var module = SampleInputLoader.LoadAssembly("others").MainModule;
			var type = module.GetType ("Others");

			Assert.IsTrue (type.HasProperties);

			var properties = type.Properties;

			Assert.AreEqual (1, properties.Count);

			var property = properties [0];

			Assert.IsNotNull (property);
			Assert.AreEqual ("Context", property.Name);
			Assert.IsNotNull (property.PropertyType);
			Assert.AreEqual ("System.String", property.PropertyType.FullName);

			Assert.IsTrue (property.HasOtherMethods);

			Assert.AreEqual (2, property.OtherMethods.Count);

			var other = property.OtherMethods [0];
			Assert.AreEqual ("let_Context", other.Name);

			other = property.OtherMethods [1];
			Assert.AreEqual ("bet_Context", other.Name);
		}

		[TestMethod]
        public void SetOnlyIndexer()
        {
            var module = SampleInputLoader.LoadAssembly("Properties").MainModule;
			var type = module.GetType ("Bar");
			var indexer = type.Properties.Where (property => property.Name == "Item").First ();

			var parameters = indexer.Parameters;

			Assert.AreEqual (2, parameters.Count);
			Assert.AreEqual ("System.Int32", parameters [0].ParameterType.FullName);
			Assert.AreEqual ("System.String", parameters [1].ParameterType.FullName);
		}

		[TestMethod]
        public void ReadSemanticsFirst()
        {
            var module = SampleInputLoader.LoadAssembly("Properties").MainModule;
			var type = module.GetType ("Baz");
			var setter = type.GetMethod ("set_Bingo");

			Assert.AreEqual (MethodSemanticsAttributes.Setter, setter.SemanticsAttributes);

			var property = type.Properties.Where (p => p.Name == "Bingo").First ();

			Assert.AreEqual (setter, property.SetMethod);
			Assert.AreEqual (type.GetMethod ("get_Bingo"), property.GetMethod);
		}
	}
}
