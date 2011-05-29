// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Text;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Static helper methods for reflection names.
	/// </summary>
	public static class ReflectionHelper
	{
		/// <summary>
		/// A reflection class used to represent <c>null</c>.
		/// </summary>
		public sealed class Null {}
		
		/// <summary>
		/// A reflection class used to represent <c>dynamic</c>.
		/// </summary>
		public sealed class Dynamic {}
		
		#region ITypeResolveContext.GetClass(Type)
		/// <summary>
		/// Retrieves a class.
		/// </summary>
		/// <returns>Returns the class; or null if it is not found.</returns>
		public static TypeDefinition GetClass(this ITypeResolveContext context, Type type)
		{
			if (type == null)
				return null;
			while (type.IsArray || type.IsPointer || type.IsByRef)
				type = type.GetElementType();
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
				type = type.GetGenericTypeDefinition();
			if (type.IsGenericParameter)
				return null;
			if (type.DeclaringType != null) {
				TypeDefinition declaringType = GetClass(context, type.DeclaringType);
				if (declaringType != null) {
					int typeParameterCount;
					string name = SplitTypeParameterCountFromReflectionName(type.Name, out typeParameterCount);
					typeParameterCount += declaringType.TypeParameterCount;
					foreach (TypeDefinition innerClass in declaringType.InnerClasses) {
						if (innerClass.Name == name && innerClass.TypeParameterCount == typeParameterCount) {
							return innerClass;
						}
					}
				}
				return null;
			} else {
				int typeParameterCount;
				string name = SplitTypeParameterCountFromReflectionName(type.Name, out typeParameterCount);
				return context.GetClass(type.Namespace, name, typeParameterCount, StringComparer.Ordinal);
			}
		}
		#endregion
		
		#region Type.ToTypeReference()
		/// <summary>
		/// Creates a reference to the specified type.
		/// </summary>
		/// <param name="type">The type to be converted.</param>
		/// <param name="entity">The parent entity, used to fetch the ITypeParameter for generic types.</param>
		/// <returns>Returns the type reference.</returns>
		public static ITypeReference ToTypeReference(this Type type, IEntity entity = null)
		{
            throw new NotSupportedException();

			if (type == null)
				return SharedTypes.UnknownType;
			if (type.IsGenericType && !type.IsGenericTypeDefinition) {
				ITypeReference def = ToTypeReference(type.GetGenericTypeDefinition(), entity);
				Type[] arguments = type.GetGenericArguments();
				ITypeReference[] args = new ITypeReference[arguments.Length];
				for (int i = 0; i < arguments.Length; i++) {
					args[i] = ToTypeReference(arguments[i], entity);
				}
				return new ParameterizedTypeReference(def, args);
			} else if (type.IsArray) {
				return new ArrayTypeReference(ToTypeReference(type.GetElementType(), entity), type.GetArrayRank());
			} else if (type.IsPointer) {
				return new PointerTypeReference(ToTypeReference(type.GetElementType(), entity));
			} else if (type.IsByRef) {
                throw new NotSupportedException();
				//return new ByReferenceTypeReference(ToTypeReference(type.GetElementType(), entity));
			} else if (type.IsGenericParameter) {
				if (type.DeclaringMethod != null) {
					return SharedTypes.UnknownType;
				} else {
					TypeDefinition c = (entity as TypeDefinition) ?? (entity != null ? entity.DeclaringTypeDefinition : null);
					if (c != null && type.GenericParameterPosition < c.TypeParameters.Count) {
						if (c.TypeParameters[type.GenericParameterPosition].Name == type.Name) {
							return c.TypeParameters[type.GenericParameterPosition];
						}
					}
					return SharedTypes.UnknownType;
				}
			} else if (type.DeclaringType != null) {
				if (type == typeof(Dynamic))
					return SharedTypes.Dynamic;
				else if (type == typeof(Null))
					return SharedTypes.Null;
				ITypeReference baseTypeRef = ToTypeReference(type.DeclaringType, entity);
				int typeParameterCount;
				string name = SplitTypeParameterCountFromReflectionName(type.Name, out typeParameterCount);
				return new NestedTypeReference(baseTypeRef, name, typeParameterCount);
			} else {
				int typeParameterCount;
				string name = SplitTypeParameterCountFromReflectionName(type.Name, out typeParameterCount);
				return new GetClassTypeReference(type.Namespace, name, typeParameterCount);
			}
		}
		#endregion
		
		#region SplitTypeParameterCountFromReflectionName
		/// <summary>
		/// Removes the ` with type parameter count from the reflection name.
		/// </summary>
		/// <remarks>Do not use this method with the full name of inner classes.</remarks>
		public static string SplitTypeParameterCountFromReflectionName(string reflectionName)
		{
			int pos = reflectionName.LastIndexOf('`');
			if (pos < 0) {
				return reflectionName;
			} else {
				return reflectionName.Substring(0, pos);
			}
		}
		
		/// <summary>
		/// Removes the ` with type parameter count from the reflection name.
		/// </summary>
		/// <remarks>Do not use this method with the full name of inner classes.</remarks>
		public static string SplitTypeParameterCountFromReflectionName(string reflectionName, out int typeParameterCount)
		{
			int pos = reflectionName.LastIndexOf('`');
			if (pos < 0) {
				typeParameterCount = 0;
				return reflectionName;
			} else {
				string typeCount = reflectionName.Substring(pos + 1);
				if (int.TryParse(typeCount, out typeParameterCount))
					return reflectionName.Substring(0, pos);
				else
					return reflectionName;
			}
		}
		#endregion
		
		#region TypeCode.ToTypeReference()
		static readonly ITypeReference[] primitiveTypeReferences = {
			SharedTypes.UnknownType, // TypeCode.Empty
			KnownTypeReference.Object,
			new GetClassTypeReference("System", "DBNull", 0),
			KnownTypeReference.Boolean,
			KnownTypeReference.Char,
			KnownTypeReference.SByte,
			KnownTypeReference.Byte,
			KnownTypeReference.Int16,
			KnownTypeReference.UInt16,
			KnownTypeReference.Int32,
			KnownTypeReference.UInt32,
			KnownTypeReference.Int64,
			KnownTypeReference.UInt64,
			KnownTypeReference.Single,
			KnownTypeReference.Double,
			new GetClassTypeReference("System", "Decimal", 0),
			new GetClassTypeReference("System", "DateTime", 0),
			SharedTypes.UnknownType, // (TypeCode)17 has no enum value?
			KnownTypeReference.String
		};
		
		/// <summary>
		/// Creates a reference to the specified type.
		/// </summary>
		/// <param name="typeCode">The type to be converted.</param>
		/// <returns>Returns the type reference.</returns>
		public static ITypeReference ToTypeReference(this TypeCode typeCode)
		{
			return primitiveTypeReferences[(int)typeCode];
		}
		#endregion
		
		#region GetTypeCode
		static readonly Dictionary<string, TypeCode> typeNameToCodeDict = new Dictionary<string, TypeCode> {
			{ "Object",   TypeCode.Object },
			{ "DBNull",   TypeCode.DBNull },
			{ "Boolean",  TypeCode.Boolean },
			{ "Char",     TypeCode.Char },
			{ "SByte",    TypeCode.SByte },
			{ "Byte",     TypeCode.Byte },
			{ "Int16",    TypeCode.Int16 },
			{ "UInt16",   TypeCode.UInt16 },
			{ "Int32",    TypeCode.Int32 },
			{ "UInt32",   TypeCode.UInt32 },
			{ "Int64",    TypeCode.Int64 },
			{ "UInt64",   TypeCode.UInt64 },
			{ "Single",   TypeCode.Single },
			{ "Double",   TypeCode.Double },
			{ "Decimal",  TypeCode.Decimal },
			{ "DateTime", TypeCode.DateTime },
			{ "String",   TypeCode.String }
		};
		
		/// <summary>
		/// Gets the type code for the specified type, or TypeCode.Empty if none of the other type codes matches.
		/// </summary>
		public static TypeCode GetTypeCode(IType type)
		{
			TypeDefinition def = type as TypeDefinition;
			TypeCode typeCode;
			if (def != null && def.TypeParameterCount == 0 && def.Namespace == "System" && typeNameToCodeDict.TryGetValue(def.Name, out typeCode))
				return typeCode;
			else
				return TypeCode.Empty;
		}
		#endregion
		
		#region ParseReflectionName
		/// <summary>
		/// Parses a reflection name into a type reference.
		/// </summary>
		/// <param name="reflectionTypeName">The reflection name of the type.</param>
		/// <param name="parentEntity">Parent entity, used to find the type parameters for open types.
		/// If no entity is provided, type parameters are converted to <see cref="SharedTypes.UnknownType"/>.</param>
		/// <exception cref="ReflectionNameParseException">The syntax of the reflection type name is invalid</exception>
		/// <returns>A type reference that represents the reflection name.</returns>
		public static ITypeReference ParseReflectionName(string reflectionTypeName, IEntity parentEntity = null)
		{
            throw new NotSupportedException();
		}
		
		static bool IsReflectionNameSpecialCharacter(char c)
		{
			switch (c) {
				case '+':
				case '`':
				case '[':
				case ']':
				case ',':
				case '*':
				case '&':
					return true;
				default:
					return false;
			}
		}
		
		static string ReadTypeName(string reflectionTypeName, ref int pos, out int tpc)
		{
			int startPos = pos;
			// skip the simple name portion:
			while (pos < reflectionTypeName.Length && !IsReflectionNameSpecialCharacter(reflectionTypeName[pos]))
				pos++;
			if (pos == startPos)
				throw new ReflectionNameParseException(pos, "Expected type name");
			string typeName = reflectionTypeName.Substring(startPos, pos - startPos);
			if (pos < reflectionTypeName.Length && reflectionTypeName[pos] == '`') {
				pos++;
				tpc = ReadTypeParameterCount(reflectionTypeName, ref pos);
			} else {
				tpc = 0;
			}
			return typeName;
		}
		
		static int ReadTypeParameterCount(string reflectionTypeName, ref int pos)
		{
			int startPos = pos;
			while (pos < reflectionTypeName.Length) {
				char c = reflectionTypeName[pos];
				if (c < '0' || c > '9')
					break;
				pos++;
			}
			int tpc;
			if (!int.TryParse(reflectionTypeName.Substring(startPos, pos - startPos), out tpc))
				throw new ReflectionNameParseException(pos, "Expected type parameter count");
			return tpc;
		}
		#endregion
	}
}
