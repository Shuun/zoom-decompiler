// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Mi.NRefactory.TypeSystem;

namespace Mi.CSharp.Resolver
{
	/// <summary>
	/// Represents a group of methods.
	/// </summary>
	public class MethodGroupResolveResult : ResolveResult
	{
		readonly ReadOnlyCollection<IType> typeArguments;
		readonly IType targetType;
		readonly string methodName;
		
		public MethodGroupResolveResult(IType targetType, string methodName, IList<IType> typeArguments) : base(SharedTypes.UnknownType)
		{
			if (targetType == null)
				throw new ArgumentNullException("targetType");
			this.targetType = targetType;
			this.methodName = methodName;
            this.typeArguments = typeArguments != null ? new ReadOnlyCollection<IType>(typeArguments) : Empty.ReadOnlyCollection<IType>();
		}
		
		public IType TargetType {
			get { return targetType; }
		}
		
		public string MethodName {
			get { return methodName; }
		}
		
		public ReadOnlyCollection<IType> TypeArguments {
			get { return typeArguments; }
		}
		
		public override string ToString()
		{
			return string.Format("[{0}]", GetType().Name);
		}
	}
}
