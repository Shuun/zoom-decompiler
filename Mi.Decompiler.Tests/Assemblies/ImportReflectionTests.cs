using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SR = System.Reflection;
using System.Runtime.CompilerServices;

using Mi.Assemblies.Cil;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mi.Assemblies.Tests {

	[TestClass]
	public class ImportReflectionTests 
    {

		[TestMethod]
		public void ImportString ()
		{
			var get_string = Compile<Func<string>> ((_, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldstr, "yo dawg!");
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("yo dawg!", get_string ());
		}

		[TestMethod]
		public void ImportInt ()
		{
			var add = Compile<Func<int, int, int>> ((_, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Add);
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, add (40, 2));
		}

		[TestMethod]
		public void ImportStringByRef ()
		{
			var get_string = Compile<Func<string, string>> ((module, body) => {
				var type = module.Types [1];

				var method_by_ref = new MethodDefinition(
                    "ModifyString",
                    MethodAttributes.Private | MethodAttributes.Static,
                    module.Import (typeof (void)));

				type.Methods.Add (method_by_ref);

				method_by_ref.Parameters.Add (new ParameterDefinition (module.Import (typeof (string))));
				method_by_ref.Parameters.Add (new ParameterDefinition (module.Import (typeof (string).MakeByRefType ())));

				var m_il = method_by_ref.Body.GetILProcessor ();
				m_il.Emit (OpCodes.Ldarg_1);
				m_il.Emit (OpCodes.Ldarg_0);
				m_il.Emit (OpCodes.Stind_Ref);
				m_il.Emit (OpCodes.Ret);

				var v_0 = new VariableDefinition (module.Import (typeof (string)));
				body.Variables.Add (v_0);

				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldnull);
				il.Emit (OpCodes.Stloc, v_0);
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldloca, v_0);
				il.Emit (OpCodes.Call, method_by_ref);
				il.Emit (OpCodes.Ldloc_0);
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("foo", get_string ("foo"));
		}

		[TestMethod]
		public void ImportStringArray ()
		{
			var identity = Compile<Func<string [,], string [,]>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ret);
			});

			var array = new string [2, 2];

			Assert.AreEqual (array, identity (array));
		}

		[TestMethod]
		public void ImportFieldStringEmpty ()
		{
			var get_empty = Compile<Func<string>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldsfld, module.Import (typeof (string).GetField ("Empty")));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("", get_empty ());
		}

		[TestMethod]
		public void ImportStringConcat ()
		{
			var concat = Compile<Func<string, string, string>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Call, module.Import (typeof (string).GetMethod ("Concat", new [] { typeof (string), typeof (string) })));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("FooBar", concat ("Foo", "Bar"));
		}

		public class Generic<T> {
			public T Field;

			public T Method (T t)
			{
				return t;
			}

			public TS GenericMethod<TS> (T t, TS s)
			{
				return s;
			}

			public Generic<TS> ComplexGenericMethod<TS> (T t, TS s)
			{
				return new Generic<TS> { Field = s };
			}
		}

		[TestMethod]
		public void ImportGenericField ()
		{
			var get_field = Compile<Func<Generic<string>, string>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldfld, module.Import (typeof (Generic<string>).GetField ("Field")));
				il.Emit (OpCodes.Ret);
			});

			var generic = new Generic<string> {
				Field = "foo",
			};

			Assert.AreEqual ("foo", get_field (generic));
		}

		[TestMethod]
		public void ImportGenericMethod ()
		{
			var generic_identity = Compile<Func<Generic<int>, int, int>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Callvirt, module.Import (typeof (Generic<int>).GetMethod ("Method")));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, generic_identity (new Generic<int> (), 42));
		}

		[TestMethod]
		public void ImportGenericMethodSpec ()
		{
			var gen_spec_id = Compile<Func<Generic<string>, int, int>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldnull);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Callvirt, module.Import (typeof (Generic<string>).GetMethod ("GenericMethod").MakeGenericMethod (typeof (int))));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, gen_spec_id (new Generic<string> (), 42));
		}

		[TestMethod]
		public void ImportComplexGenericMethodSpec ()
		{
			var gen_spec_id = Compile<Func<Generic<string>, int, int>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldnull);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Callvirt, module.Import (typeof (Generic<string>).GetMethod ("ComplexGenericMethod").MakeGenericMethod (typeof (int))));
				il.Emit (OpCodes.Ldfld, module.Import (typeof (Generic<string>).GetField ("Field")));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, gen_spec_id (new Generic<string> (), 42));
		}

		public class Foo<TFoo> {
			public List<TFoo> list;
		}

		[TestMethod]
		public void ImportGenericTypeDefOrOpen ()
		{
			var module = typeof (Foo<>).ToDefinition ().Module;

			var foo_def = module.Import (typeof (Foo<>));
			var foo_open = module.Import (typeof (Foo<>), foo_def);

			Assert.AreEqual ("Mi.Assemblies.Tests.ImportReflectionTests/Foo`1", foo_def.FullName);
			Assert.AreEqual ("Mi.Assemblies.Tests.ImportReflectionTests/Foo`1<TFoo>", foo_open.FullName);
		}

		[TestMethod]
		public void ImportGenericTypeFromContext ()
		{
			var list_foo = typeof (Foo<>).GetField ("list").FieldType;
			var generic_list_foo_open = typeof (Generic<>).MakeGenericType (list_foo);

			var foo_def = typeof (Foo<>).ToDefinition ();
			var module = foo_def.Module;

			var generic_foo = module.Import (generic_list_foo_open, foo_def);

			Assert.AreEqual ("Mi.Assemblies.Tests.ImportReflectionTests/Generic`1<System.Collections.Generic.List`1<TFoo>>",
				generic_foo.FullName);
		}

		[TestMethod]
		public void ImportGenericTypeDefFromContext ()
		{
            var asm = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("A", new Version(0, 0)), "A", ModuleKind.Dll);

			var foo_open = typeof (Foo<>).MakeGenericType (typeof (Foo<>).GetGenericArguments () [0]);
			var generic_foo_open = typeof (Generic<>).MakeGenericType (foo_open);

            var foo_ref = asm.MainModule.Import(typeof(Foo<>));
			var module = asm.MainModule;

			var generic_foo = module.Import (generic_foo_open, foo_ref);

			Assert.AreEqual ("Mi.Assemblies.Tests.ImportReflectionTests/Generic`1<Mi.Assemblies.Tests.ImportReflectionTests/Foo`1<TFoo>>",
				generic_foo.FullName);
		}

		[TestMethod]
		public void ImportArrayTypeDefFromContext ()
		{
            var asm = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition ("A", new Version(0,0)), "A", ModuleKind.Dll);

			var foo_open = typeof (Foo<>).MakeGenericType (typeof (Foo<>).GetGenericArguments () [0]);
			var foo_open_array = foo_open.MakeArrayType ();

			var foo_ref = asm.MainModule.Import(typeof (Foo<>));
			var module = asm.MainModule;

			var array_foo = module.Import (foo_open_array, foo_ref);

            Assert.AreEqual("Mi.Assemblies.Tests.ImportReflectionTests/Foo`1<TFoo>[]",
				array_foo.FullName);
		}

		[TestMethod]
		public void ImportGenericFieldFromContext ()
		{
			var list_foo = typeof (Foo<>).GetField ("list").FieldType;
			var generic_list_foo_open = typeof (Generic<>).MakeGenericType (list_foo);
			var generic_list_foo_open_field = generic_list_foo_open.GetField ("Field");

			var foo_def = typeof (Foo<>).ToDefinition ();
			var module = foo_def.Module;

			var generic_field = module.Import (generic_list_foo_open_field, foo_def);

			Assert.AreEqual ("TFoo Mi.Assemblies.Tests.ImportReflectionTests/Generic`1<System.Collections.Generic.List`1<TFoo>>::Field",
				generic_field.FullName);
		}

		[TestMethod]
		public void ImportGenericMethodFromContext ()
		{
			var list_foo = typeof (Foo<>).GetField ("list").FieldType;
			var generic_list_foo_open = typeof (Generic<>).MakeGenericType (list_foo);
			var generic_list_foo_open_method = generic_list_foo_open.GetMethod ("Method");

			var foo_def = typeof (Foo<>).ToDefinition ();
			var module = foo_def.Module;

			var generic_method = module.Import (generic_list_foo_open_method, foo_def);

			Assert.AreEqual ("TFoo Mi.Assemblies.Tests.ImportReflectionTests/Generic`1<System.Collections.Generic.List`1<TFoo>>::Method(TFoo)",
				generic_method.FullName);
		}

		[TestMethod]
		public void ImportMethodOnOpenGenericType ()
		{
			var module = typeof (Generic<>).ToDefinition ().Module;

			var method = module.Import (typeof (Generic<>).GetMethod ("Method"));

			Assert.AreEqual ("T Mi.Assemblies.Tests.ImportReflectionTests/Generic`1<T>::Method(T)", method.FullName);
		}

		[TestMethod]
		public void ImportGenericMethodOnOpenGenericType ()
		{
			var module = typeof (Generic<>).ToDefinition ().Module;

			var generic_method = module.Import (typeof (Generic<>).GetMethod ("GenericMethod"));

			Assert.AreEqual ("TS Mi.Assemblies.Tests.ImportReflectionTests/Generic`1<T>::GenericMethod(T,TS)", generic_method.FullName);

			generic_method = module.Import (typeof (Generic<>).GetMethod ("GenericMethod"), generic_method);

			Assert.AreEqual ("TS Mi.Assemblies.Tests.ImportReflectionTests/Generic`1<T>::GenericMethod<TS>(T,TS)", generic_method.FullName);
		}

		delegate void Emitter (ModuleDefinition module, MethodBody body);

		[MethodImpl (MethodImplOptions.NoInlining)]
		static TDelegate Compile<TDelegate> (Emitter emitter)
			where TDelegate : class
		{
			var name = GetTestCaseName ();

			var module = CreateTestModule<TDelegate> (name, emitter);
			var assembly = LoadTestModule (module);

			return CreateRunDelegate<TDelegate> (GetTestCase (name, assembly));
		}

		static TDelegate CreateRunDelegate<TDelegate> (Type type)
			where TDelegate : class
		{
			return (TDelegate) (object) Delegate.CreateDelegate (typeof (TDelegate), type.GetMethod ("Run"));
		}

		static Type GetTestCase (string name, SR.Assembly assembly)
		{
			return assembly.GetType (name);
		}

		static SR.Assembly LoadTestModule (ModuleDefinition module)
		{
			using (var stream = new MemoryStream ()) {
				module.Write (stream);
				//File.WriteAllBytes (Path.Combine (Path.Combine (Path.GetTempPath (), "cecil"), module.Name + ".dll"), stream.ToArray ());
                var p = new System.Windows.AssemblyPart();
                var result = p.Load(stream);
                return result; // SR.Assembly.Load(stream.ToArray());
			}
		}

		static ModuleDefinition CreateTestModule<TDelegate> (string name, Emitter emitter)
		{
			var module = CreateModule (name);

			var type = new TypeDefinition (
				"",
				name,
				TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract,
				module.Import (typeof (object)));

			module.Types.Add (type);

			var method = CreateMethod (type, typeof (TDelegate).GetMethod ("Invoke"));

			emitter (module, method.Body);

			return module;
		}

		static MethodDefinition CreateMethod (TypeDefinition type, SR.MethodInfo pattern)
		{
			var module = type.Module;

			var method = new MethodDefinition(
                "Run",
                MethodAttributes.Public | MethodAttributes.Static,
                module.Import (pattern.ReturnType));

			type.Methods.Add (method);

			foreach (var parameter_pattern in pattern.GetParameters ())
				method.Parameters.Add (new ParameterDefinition (module.Import (parameter_pattern.ParameterType)));

			return method;
		}

		static ModuleDefinition CreateModule (string name)
		{
			return ModuleDefinition.CreateModule (name, ModuleKind.Dll);
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		static string GetTestCaseName ()
		{
			var stack_trace = new StackTrace ();
			var stack_frame = stack_trace.GetFrame (2);

			return "ImportReflection_" + stack_frame.GetMethod ().Name;
		}
	}
}
