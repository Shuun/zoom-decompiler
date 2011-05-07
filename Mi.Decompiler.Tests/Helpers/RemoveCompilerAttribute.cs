using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mi.Decompiler.Ast.Transforms;
using Mi.NRefactory.CSharp;

namespace Mi.Decompiler.Tests.Helpers
{
	class RemoveCompilerAttribute : DepthFirstAstVisitor<object, object>, IAstTransform
	{
		public override object VisitAttribute(NRefactory.CSharp.Attribute attribute, object data)
		{
			var section = (AttributeSection)attribute.Parent;
			SimpleType type = attribute.Type as SimpleType;
			if (section.AttributeTarget == "assembly" &&
				(type.Identifier == "CompilationRelaxations" 
                || type.Identifier == "RuntimeCompatibility"
                || type.Identifier == "TargetFramework")
                || type.Identifier == "Debuggable")
			{
				attribute.Remove();
				if (section.Attributes.Count == 0)
					section.Remove();
			}
			if (section.AttributeTarget == "module" && type.Identifier == "UnverifiableCode")
			{
				attribute.Remove();
				if (section.Attributes.Count == 0)
					section.Remove();
			}
			return null;
		}

		public void Run(AstNode node)
		{
			node.AcceptVisitor(this, null);
		}
	}
}
