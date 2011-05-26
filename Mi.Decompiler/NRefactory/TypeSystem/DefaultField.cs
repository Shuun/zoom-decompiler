// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="IField"/>.
	/// </summary>
	public class Field : AbstractMember, IField
	{
		IConstantValue constantValue;
		
		protected override void FreezeInternal()
		{
			if (constantValue != null)
				constantValue.Freeze();
			base.FreezeInternal();
		}
		
		public Field(ITypeDefinition declaringTypeDefinition, string name)
			: base(declaringTypeDefinition, name, EntityType.Field)
		{
		}
		
		protected Field(IField f) : base(f)
		{
			this.constantValue = f.ConstantValue;
			this.IsReadOnly = f.IsReadOnly;
			this.IsVolatile = f.IsVolatile;
		}
		
		public override void ApplyInterningProvider(IInterningProvider provider)
		{
			base.ApplyInterningProvider(provider);
			if (provider != null)
				constantValue = provider.Intern(constantValue);
		}
		
		public bool IsConst {
			get { return constantValue != null; }
		}
		
		public bool IsReadOnly {
			get { return (flags & MemberFlags.ReadOnly)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.ReadOnly :
                    flags & ~ MemberFlags.ReadOnly;
			}
		}
		
		public bool IsVolatile {
			get { return (flags & MemberFlags.Volatile)!=0; }
			set {
				CheckBeforeMutation();
				flags = value ?
                    flags | MemberFlags.Volatile :
                    flags & ~MemberFlags.Volatile;
			}
		}
		
		public IConstantValue ConstantValue {
			get { return constantValue; }
			set {
				CheckBeforeMutation();
				constantValue = value;
			}
		}
		
		ITypeReference IVariable.Type {
			get { return this.ReturnType; }
		}
	}
}
