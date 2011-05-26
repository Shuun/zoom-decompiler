// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Property"/>.
	/// </summary>
    public class Property : AbstractMember, IParameterizedMember
	{
		Accessor getter, setter;
		IList<Parameter> parameters;
		
		protected override void FreezeInternal()
		{
			parameters = FreezeList(parameters);
			if (getter != null) getter.Freeze();
			if (setter != null) setter.Freeze();
			base.FreezeInternal();
		}
		
		public Property(TypeDefinition declaringTypeDefinition, string name)
			: base(declaringTypeDefinition, name, EntityType.Property)
		{
		}
		
		protected Property(Property p) : base(p)
		{
			this.getter = p.Getter;
			this.setter = p.Setter;
			this.parameters = CopyList(p.Parameters);
		}
		
		public override void ApplyInterningProvider(IInterningProvider provider)
		{
			base.ApplyInterningProvider(provider);
			if (provider != null) {
				getter = provider.Intern(getter);
				setter = provider.Intern(setter);
				parameters = provider.InternList(parameters);
			}
		}
		
		public bool IsIndexer {
			get { return this.EntityType == EntityType.Indexer; }
		}
		
		public IList<Parameter> Parameters {
			get {
				if (parameters == null)
					parameters = new List<Parameter>();
				return parameters;
			}
		}
		
		public bool CanGet {
			get { return getter != null; }
		}
		
		public bool CanSet {
			get { return setter != null; }
		}
		
		public Accessor Getter{
			get { return getter; }
			set {
				CheckBeforeMutation();
				getter = value;
			}
		}
		
		public Accessor Setter {
			get { return setter; }
			set {
				CheckBeforeMutation();
				setter = value;
			}
		}
	}
}
