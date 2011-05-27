// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Text;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Method" /> interface.
	/// </summary>
	public class Method : AbstractMember, IParameterizedMember
	{
		IList<IAttribute> returnTypeAttributes;
		IList<ITypeParameter> typeParameters;
		IList<Parameter> parameters;
		
		protected override void FreezeInternal()
		{
			returnTypeAttributes = FreezeList(returnTypeAttributes);
			typeParameters = FreezeList(typeParameters);
			parameters = FreezeList(parameters);
			base.FreezeInternal();
		}
		
		public Method(TypeDefinition declaringTypeDefinition, string name)
			: base(declaringTypeDefinition, name, EntityType.Method)
		{
            throw new NotSupportedException("Method class is disabled.");
		}
		
		/// <summary>
		/// Copy constructor
		/// </summary>
		protected Method(Method method) : base(method)
		{
			returnTypeAttributes = CopyList(returnTypeAttributes);
			typeParameters = CopyList(typeParameters);
			parameters = CopyList(parameters);
			this.IsExtensionMethod = method.IsExtensionMethod;
		}
		
		public override void ApplyInterningProvider(IInterningProvider provider)
		{
			base.ApplyInterningProvider(provider);
			if (provider != null) {
				returnTypeAttributes = provider.InternList(returnTypeAttributes);
				typeParameters = provider.InternList(typeParameters);
				parameters = provider.InternList(parameters);
			}
		}
		
		public IList<IAttribute> ReturnTypeAttributes {
			get {
				if (returnTypeAttributes == null)
					returnTypeAttributes = new List<IAttribute>();
				return returnTypeAttributes;
			}
		}
		
		public IList<ITypeParameter> TypeParameters {
			get {
				if (typeParameters == null)
					typeParameters = new List<ITypeParameter>();
				return typeParameters;
			}
		}
		
		public bool IsExtensionMethod {
			get { return (flags & MemberFlags.ExtensionMethod)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.ExtensionMethod :
                    flags & ~MemberFlags.ExtensionMethod;
			}
		}
		
		public bool IsConstructor {
			get { return this.EntityType == EntityType.Constructor; }
		}
		
		public bool IsDestructor {
			get { return this.EntityType == EntityType.Destructor; }
		}
		
		public bool IsOperator {
			get { return this.EntityType == EntityType.Operator; }
		}
		
		public IList<Parameter> Parameters {
			get {
				if (parameters == null)
					parameters = new List<Parameter>();
				return parameters;
			}
		}
		
		public override string ToString()
		{
			StringBuilder b = new StringBuilder("[");
			b.Append(EntityType.ToString());
			b.Append(' ');
			b.Append(DeclaringType.Name);
			b.Append('.');
			b.Append(Name);
			b.Append('(');
			var p = this.Parameters;
			for (int i = 0; i < p.Count; i++) {
				if (i > 0) b.Append(", ");
				b.Append(p[i].ToString());
			}
			b.Append("):");
			b.Append(ReturnType.ToString());
			b.Append(']');
			return b.ToString();
		}
		
		public static Method CreateDefaultConstructor(TypeDefinition typeDefinition)
		{
            throw new NotSupportedException("Method class is disabled.");
		}
	}
}
