// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Field"/>.
	/// </summary>
	public class Field : AbstractMember, IVariable
	{
		IConstantValue constantValue;
		
		protected override void FreezeInternal()
		{
			if (constantValue != null)
				constantValue.Freeze();
			base.FreezeInternal();
		}
		
		public Field(TypeDefinition declaringTypeDefinition, string name)
			: base(declaringTypeDefinition, name, EntityType.Field)
		{
            throw new NotSupportedException();
		}
		
		protected Field(Field f) : base(f)
		{
            throw new NotSupportedException();
		}
		
		public override void ApplyInterningProvider(IInterningProvider provider)
		{
            throw new NotSupportedException();
		}
		
		public bool IsConst {
            get { throw new NotSupportedException(); }
		}
		
		public bool IsReadOnly {
            get { throw new NotSupportedException(); }
			set {
                throw new NotSupportedException();
			}
		}
		
		public bool IsVolatile {
            get { throw new NotSupportedException(); }
			set {
                throw new NotSupportedException();
			}
		}
		
		public IConstantValue ConstantValue {
            get { throw new NotSupportedException(); }
			set {
                throw new NotSupportedException();
			}
		}
		
		ITypeReference IVariable.Type {
            get { throw new NotSupportedException(); }
		}
	}
}
