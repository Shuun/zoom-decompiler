// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mi.NRefactory.CSharp.Resolver;
using Mi.NRefactory.TypeSystem;
using Mi.NRefactory.TypeSystem.Implementation;
using Mi.NRefactory.Utils;
using Mi.NRefactory.CSharp.Resolver.ConstantValues;

namespace Mi.NRefactory.CSharp
{
	/// <summary>
	/// Produces type and member definitions from the DOM.
	/// </summary>
	public static class TypeSystemConvertVisitor
	{
		internal static ITypeReference ConvertType(AstType type, ITypeDefinition parentTypeDefinition, IMethod parentMethodDefinition, UsingScope parentUsingScope, bool isInUsingDeclaration)
		{
			SimpleType s = type as SimpleType;
			if (s != null) {
				List<ITypeReference> typeArguments = new List<ITypeReference>();
				foreach (var ta in s.TypeArguments) {
					typeArguments.Add(ConvertType(ta, parentTypeDefinition, parentMethodDefinition, parentUsingScope, isInUsingDeclaration));
				}
				if (typeArguments.Count == 0 && parentMethodDefinition != null) {
					// SimpleTypeOrNamespaceReference doesn't support method type parameters,
					// so we directly handle them here.
					foreach (ITypeParameter tp in parentMethodDefinition.TypeParameters) {
						if (tp.Name == s.Identifier)
							return tp;
					}
				}
				return new SimpleTypeOrNamespaceReference(s.Identifier, typeArguments, parentTypeDefinition, parentUsingScope, isInUsingDeclaration);
			}
			
			PrimitiveType p = type as PrimitiveType;
			if (p != null) {
				switch (p.Keyword) {
					case "string":
						return KnownTypeReference.String;
					case "int":
						return KnownTypeReference.Int32;
					case "uint":
						return KnownTypeReference.UInt32;
					case "object":
						return KnownTypeReference.Object;
					case "bool":
						return KnownTypeReference.Boolean;
					case "sbyte":
						return KnownTypeReference.SByte;
					case "byte":
						return KnownTypeReference.Byte;
					case "short":
						return KnownTypeReference.Int16;
					case "ushort":
						return KnownTypeReference.UInt16;
					case "long":
						return KnownTypeReference.Int64;
					case "ulong":
						return KnownTypeReference.UInt64;
					case "float":
						return KnownTypeReference.Single;
					case "double":
						return KnownTypeReference.Double;
					case "decimal":
						return ReflectionHelper.ToTypeReference(TypeCode.Decimal);
					case "char":
						return KnownTypeReference.Char;
					case "void":
						return KnownTypeReference.Void;
					default:
						return SharedTypes.UnknownType;
				}
			}
			MemberType m = type as MemberType;
			if (m != null) {
				ITypeOrNamespaceReference t;
				if (m.IsDoubleColon) {
					SimpleType st = m.Target as SimpleType;
					if (st != null) {
						t = new AliasNamespaceReference(st.Identifier, parentUsingScope);
					} else {
						t = null;
					}
				} else {
					t = ConvertType(m.Target, parentTypeDefinition, parentMethodDefinition, parentUsingScope, isInUsingDeclaration) as ITypeOrNamespaceReference;
				}
				if (t == null)
					return SharedTypes.UnknownType;
				List<ITypeReference> typeArguments = new List<ITypeReference>();
				foreach (var ta in m.TypeArguments) {
					typeArguments.Add(ConvertType(ta, parentTypeDefinition, parentMethodDefinition, parentUsingScope, isInUsingDeclaration));
				}
				return new MemberTypeOrNamespaceReference(t, m.MemberName, typeArguments, parentTypeDefinition, parentUsingScope);
			}
			ComposedType c = type as ComposedType;
			if (c != null) {
				ITypeReference t = ConvertType(c.BaseType, parentTypeDefinition, parentMethodDefinition, parentUsingScope, isInUsingDeclaration);
				if (c.HasNullableSpecifier) {
					t = NullableType.Create(t);
				}
				for (int i = 0; i < c.PointerRank; i++) {
					t = PointerTypeReference.Create(t);
				}
				foreach (var a in c.ArraySpecifiers.Reverse()) {
					t = ArrayTypeReference.Create(t, a.Dimensions);
				}
				return t;
			}
			Debug.WriteLine("Unknown node used as type: " + type);
			return SharedTypes.UnknownType;
		}
	}
}
