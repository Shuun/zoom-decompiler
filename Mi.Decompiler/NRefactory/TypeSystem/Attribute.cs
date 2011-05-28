// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mi.NRefactory.TypeSystem.Implementation;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Default implementation of <see cref="Attribute"/>.
	/// </summary>
	public sealed class Attribute : AbstractFreezable, IAttribute, ISupportsInterning
	{
		ITypeReference attributeType;
		readonly ITypeReference[] constructorParameterTypes;
		DomRegion region;
		IList<IConstantValue> positionalArguments;
		IList<KeyValuePair<string, IConstantValue>> namedArguments;
		
		protected override void FreezeInternal()
		{
			positionalArguments = FreezeList(positionalArguments);
			
			if (namedArguments == null || namedArguments.Count == 0) {
                namedArguments = Empty.ReadOnlyCollection<KeyValuePair<string, IConstantValue>>();
			} else {
				namedArguments = Array.AsReadOnly(namedArguments.ToArray());
				foreach (var pair in namedArguments) {
					pair.Value.Freeze();
				}
			}
			
			base.FreezeInternal();
		}
		
		public Attribute(ITypeReference attributeType, IEnumerable<ITypeReference> constructorParameterTypes)
		{
            throw new NotSupportedException();

			if (attributeType == null)
				throw new ArgumentNullException("attributeType");
			this.attributeType = attributeType;
			this.constructorParameterTypes = constructorParameterTypes != null ? constructorParameterTypes.ToArray() : null;
		}
		
		public ITypeReference AttributeType {
			get { return attributeType; }
		}
		
		public ReadOnlyCollection<ITypeReference> ConstructorParameterTypes {
			get { return Array.AsReadOnly(constructorParameterTypes); }
		}
		
		public DomRegion Region {
			get { return region; }
			set {
				CheckBeforeMutation();
				region = value;
			}
		}
		
		public IList<IConstantValue> PositionalArguments {
			get {
				if (positionalArguments == null)
					positionalArguments = new List<IConstantValue>();
				return positionalArguments;
			}
		}
		
		IList<IConstantValue> IAttribute.GetPositionalArguments(ITypeResolveContext context)
		{
			return this.PositionalArguments;
		}
		
		public IList<KeyValuePair<string, IConstantValue>> NamedArguments {
			get {
				if (namedArguments == null)
					namedArguments = new List<KeyValuePair<string, IConstantValue>>();
				return namedArguments;
			}
		}
		
		IList<KeyValuePair<string, IConstantValue>> IAttribute.GetNamedArguments(ITypeResolveContext context)
		{
			return this.NamedArguments;
		}
		
		public override string ToString()
		{
			StringBuilder b = new StringBuilder();
			b.Append('[');
			b.Append(attributeType.ToString());
			if (this.PositionalArguments.Count + this.NamedArguments.Count > 0) {
				b.Append('(');
				bool first = true;
				foreach (var element in this.PositionalArguments) {
					if (first) first = false; else b.Append(", ");
					b.Append(element.ToString());
				}
				foreach (var pair in this.NamedArguments) {
					if (first) first = false; else b.Append(", ");
					b.Append(pair.Key);
					b.Append('=');
					b.Append(pair.Value.ToString());
				}
				b.Append(')');
			}
			b.Append(']');
			return b.ToString();
		}
		
		void ISupportsInterning.PrepareForInterning(IInterningProvider provider)
		{
			attributeType = provider.Intern(attributeType);
			if (constructorParameterTypes != null) {
				for (int i = 0; i < constructorParameterTypes.Length; i++) {
					constructorParameterTypes[i] = provider.Intern(constructorParameterTypes[i]);
				}
			}
			positionalArguments = provider.InternList(positionalArguments);
		}
		
		int ISupportsInterning.GetHashCodeForInterning()
		{
			return attributeType.GetHashCode() ^ (positionalArguments != null ? positionalArguments.GetHashCode() : 0) ^ (namedArguments != null ? namedArguments.GetHashCode() : 0) ^ region.GetHashCode();
		}
		
		bool ISupportsInterning.EqualsForInterning(ISupportsInterning other)
		{
			Attribute a = other as Attribute;
			return a != null && attributeType == a.attributeType && positionalArguments == a.positionalArguments && namedArguments == a.namedArguments && region == a.region;
		}
	}
}
