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
            throw new NotSupportedException("Method class is disabled.");
		}
		
		public override void ApplyInterningProvider(IInterningProvider provider)
		{
            throw new NotSupportedException("Method class is disabled.");
		}
		
		public IList<IAttribute> ReturnTypeAttributes {
			get {
                throw new NotSupportedException("Method class is disabled.");
            }
		}
		
		public IList<ITypeParameter> TypeParameters {
			get {
                throw new NotSupportedException("Method class is disabled.");
			}
		}
		
		public bool IsExtensionMethod {
            get { throw new NotSupportedException("Method class is disabled."); }
			set {
                throw new NotSupportedException("Method class is disabled.");
			}
		}
		
		public bool IsConstructor {
            get { throw new NotSupportedException("Method class is disabled."); }
		}
		
		public bool IsDestructor {
            get { throw new NotSupportedException("Method class is disabled."); }
		}
		
		public bool IsOperator {
            get { throw new NotSupportedException("Method class is disabled."); }
		}
		
		public IList<Parameter> Parameters {
			get {
                throw new NotSupportedException("Method class is disabled.");
			}
		}
		
		public override string ToString()
		{
            throw new NotSupportedException("Method class is disabled.");
		}
	}
}
