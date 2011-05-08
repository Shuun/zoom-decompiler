using System;
using System.Linq;

using Mi.Assemblies;
using Mi.Assemblies.Cil;
using Mi.Assemblies.Metadata;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mi.Decompiler.Tests;

namespace Mi.Assemblies.Tests {

	[TestClass]
	public class TypeTests
    {
		[TestMethod]
        public void TypeLayout()
        {
            var module = SampleInputLoader.LoadAssembly("Layouts").MainModule;
			var foo = module.GetType ("Foo");
			Assert.IsNotNull (foo);
			Assert.IsTrue (foo.IsValueType);

			Assert.IsTrue (foo.HasLayoutInfo);
			Assert.AreEqual (16, foo.ClassSize);

			var babar = module.GetType ("Babar");
			Assert.IsNotNull (babar);
			Assert.IsFalse (babar.IsValueType);
			Assert.IsFalse (babar.HasLayoutInfo);
		}

		[TestMethod]
        public void SimpleInterfaces()
        {
            var module = SampleInputLoader.LoadAssembly("types").MainModule;
			var ibaz = module.GetType ("IBaz");
			Assert.IsNotNull (ibaz);

			Assert.IsTrue (ibaz.HasInterfaces);

			var interfaces = ibaz.Interfaces;

			Assert.AreEqual (2, interfaces.Count);

			Assert.AreEqual ("IBar", interfaces [0].FullName);
			Assert.AreEqual ("IFoo", interfaces [1].FullName);
		}

		[TestMethod]
        public void GenericTypeDefinition()
        {
            var module = SampleInputLoader.LoadAssembly("GenericsAsm").MainModule;
			var foo = module.GetType ("Foo`2");
			Assert.IsNotNull (foo);

			Assert.IsTrue (foo.HasGenericParameters);
			Assert.AreEqual (2, foo.GenericParameters.Count);

			var tbar = foo.GenericParameters [0];

			Assert.AreEqual ("TBar", tbar.Name);
			Assert.AreEqual (foo, tbar.Owner);

			var tbaz = foo.GenericParameters [1];

			Assert.AreEqual ("TBaz", tbaz.Name);
			Assert.AreEqual (foo, tbaz.Owner);
		}

		[TestMethod]
        public void ConstrainedGenericType()
        {
            var module = SampleInputLoader.LoadAssembly("GenericsAsm").MainModule;
			var bongo_t = module.GetType ("Bongo`1");
			Assert.IsNotNull (bongo_t);

			var t = bongo_t.GenericParameters [0];
			Assert.IsNotNull (t);
			Assert.AreEqual ("T", t.Name);

			Assert.IsTrue (t.HasConstraints);
			Assert.AreEqual (2, t.Constraints.Count);

			Assert.AreEqual ("Zap", t.Constraints [0].FullName);
			Assert.AreEqual ("IZoom", t.Constraints [1].FullName);
		}

		[TestMethod]
        public void GenericBaseType()
        {
            var module = SampleInputLoader.LoadAssembly("GenericsAsm").MainModule;
			var child = module.GetType ("Child`1");

			var child_t = child.GenericParameters [0];
			Assert.IsNotNull (child_t);

			var instance = child.BaseType as GenericInstanceType;
			Assert.IsNotNull (instance);
			Assert.AreNotEqual (0, instance.MetadataToken.RID);

			Assert.AreEqual (child_t, instance.GenericArguments [0]);
		}

		[TestMethod]
        public void GenericConstraintOnGenericParameter()
        {
            var module = SampleInputLoader.LoadAssembly("GenericsAsm").MainModule;
			var duel = module.GetType ("Duel`3");

			Assert.AreEqual (3, duel.GenericParameters.Count);

			var t1 = duel.GenericParameters [0];
			var t2 = duel.GenericParameters [1];
			var t3 = duel.GenericParameters [2];

			Assert.AreEqual (t1, t2.Constraints [0]);
			Assert.AreEqual (t2, t3.Constraints [0]);
		}

		[TestMethod]
        public void GenericForwardBaseType()
        {
            var module = SampleInputLoader.LoadAssembly("GenericsAsm").MainModule;
			var tamchild = module.GetType ("TamChild");

			Assert.IsNotNull (tamchild);
			Assert.IsNotNull (tamchild.BaseType);

			var generic_instance = tamchild.BaseType as GenericInstanceType;

			Assert.IsNotNull (generic_instance);

			Assert.AreEqual (1, generic_instance.GenericArguments.Count);
			Assert.AreEqual (module.GetType ("Tamtam"), generic_instance.GenericArguments [0]);
		}

		[TestMethod]
        public void TypeExtentingGenericOfSelf()
        {
            var module = SampleInputLoader.LoadAssembly("GenericsAsm").MainModule;
			var rec_child = module.GetType ("RecChild");

			Assert.IsNotNull (rec_child);
			Assert.IsNotNull (rec_child.BaseType);

			var generic_instance = rec_child.BaseType as GenericInstanceType;

			Assert.IsNotNull (generic_instance);

			Assert.AreEqual (1, generic_instance.GenericArguments.Count);
			Assert.AreEqual (rec_child, generic_instance.GenericArguments [0]);
		}

		[TestMethod]
        public void TypeReferenceValueType()
        {
            var module = SampleInputLoader.LoadAssembly("Methods").MainModule;
			var baz = module.GetType ("Baz");
			var method = baz.GetMethod ("PrintAnswer");

			var box = method.Body.Instructions.Where (i => i.OpCode == OpCodes.Box).First ();
			var int32 = (TypeReference) box.Operand;

			Assert.IsTrue (int32.IsValueType);
		}

		[TestMethod]
        public void GenericInterfaceReference()
        {
            var module = SampleInputLoader.LoadAssembly("gifaceref").MainModule;
			var type = module.GetType ("Program");
			var iface = type.Interfaces [0];

			var instance = (GenericInstanceType) iface;
			var owner = instance.ElementType;

			Assert.AreEqual (1, instance.GenericArguments.Count);
			Assert.AreEqual (1, owner.GenericParameters.Count);
		}
	}
}
