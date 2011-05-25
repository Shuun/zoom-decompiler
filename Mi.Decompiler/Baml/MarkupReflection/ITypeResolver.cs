// Copyright (c) Cristian Civera (cristian@aspitalia.com)
// This code is distributed under the MS-PL (for details please see \doc\MS-PL.txt)

using System;
using System.Collections.Generic;
using System.Text;

namespace Mi.Decompiler.Baml.MarkupReflection
{
	public interface ITypeResolver
	{
		IType GetTypeByAssemblyQualifiedName(string name);
		IDependencyPropertyDescriptor GetDependencyPropertyDescriptor(string name, IType ownerType, IType targetType);
	}
}
