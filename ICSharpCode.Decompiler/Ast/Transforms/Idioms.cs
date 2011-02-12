﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Ast = ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp;

namespace Decompiler.Transforms.Ast
{
	public class Idioms: DepthFirstAstVisitor<object, object>
	{
		public override object VisitInvocationExpression(InvocationExpression invocationExpression, object data)
		{
			base.VisitInvocationExpression(invocationExpression, data);
			
			MethodReference methodRef = invocationExpression.Annotation<MethodReference>();
			// Reduce "String.Concat(a, b)" to "a + b"
			if (methodRef != null && methodRef.FullName == "System.String.Concat"
			    && invocationExpression.Arguments.Count() >= 2)
			{
				var arguments = invocationExpression.Arguments.ToArray();
				invocationExpression.Arguments = null; // detach arguments from invocationExpression
				Expression expr = arguments[0];
				for (int i = 1; i < arguments.Length; i++) {
					expr = new BinaryOperatorExpression(expr, BinaryOperatorType.Add, arguments[i]);
				}
				invocationExpression.ReplaceWith(expr);
			}
			
			if (methodRef != null) {
				BinaryOperatorType? bop = GetBinaryOperatorTypeFromMetadataName(methodRef.Name);
				if (bop != null && invocationExpression.Arguments.Count() == 2) {
					var arguments = invocationExpression.Arguments.ToArray();
					invocationExpression.Arguments = null; // detach arguments from invocationExpression
					invocationExpression.ReplaceWith(
						new BinaryOperatorExpression(arguments[0], bop.Value, arguments[1]).WithAnnotation(methodRef)
					);
				}
				UnaryOperatorType? uop = GetUnaryOperatorTypeFromMetadataName(methodRef.Name);
				if (uop != null && invocationExpression.Arguments.Count() == 1) {
					var arg = invocationExpression.Arguments.Single();
					arg.Remove(); // detach argument
					invocationExpression.ReplaceWith(
						new UnaryOperatorExpression(uop.Value, arg).WithAnnotation(methodRef)
					);
				}
			}
			
			return null;
		}
		
		BinaryOperatorType? GetBinaryOperatorTypeFromMetadataName(string name)
		{
			switch (name) {
				case "op_Addition":
					return BinaryOperatorType.Add;
				case "op_Subtraction":
					return BinaryOperatorType.Subtract;
				case "op_Multiply":
					return BinaryOperatorType.Multiply;
				case "op_Division":
					return BinaryOperatorType.Divide;
				case "op_Modulus":
					return BinaryOperatorType.Modulus;
				case "op_BitwiseAnd":
					return BinaryOperatorType.BitwiseAnd;
				case "op_BitwiseOr":
					return BinaryOperatorType.BitwiseOr;
				case "op_ExlusiveOr":
					return BinaryOperatorType.ExclusiveOr;
				case "op_LeftShift":
					return BinaryOperatorType.ShiftLeft;
				case "op_RightShift":
					return BinaryOperatorType.ShiftRight;
				case "op_Equality":
					return BinaryOperatorType.Equality;
				case "op_Inequality":
					return BinaryOperatorType.InEquality;
				case "op_LessThan":
					return BinaryOperatorType.LessThan;
				case "op_LessThanOrEqual":
					return BinaryOperatorType.LessThanOrEqual;
				case "op_GreaterThan":
					return BinaryOperatorType.GreaterThan;
				case "op_GreaterThanOrEqual":
					return BinaryOperatorType.GreaterThanOrEqual;
				default:
					return null;
			}
		}
		
		UnaryOperatorType? GetUnaryOperatorTypeFromMetadataName(string name)
		{
			switch (name) {
				case "op_LogicalNot":
					return UnaryOperatorType.Not;
				case  "op_OnesComplement":
					return UnaryOperatorType.BitNot;
				case "op_UnaryNegation":
					return UnaryOperatorType.Minus;
				case "op_UnaryPlus":
					return UnaryOperatorType.Plus;
				case "op_Increment":
					return UnaryOperatorType.Increment;
				case "op_Decrement":
					return UnaryOperatorType.Decrement;
				default:
					return null;
			}
		}
		
		public override object VisitAssignmentExpression(AssignmentExpression assignment, object data)
		{
			base.VisitAssignmentExpression(assignment, data);
			// First, combine "x = x op y" into "x op= y"
			BinaryOperatorExpression binary = assignment.Right as BinaryOperatorExpression;
			if (binary != null && assignment.Operator == AssignmentOperatorType.Assign) {
				if (IsWithoutSideEffects(assignment.Left) && AstComparer.AreEqual(assignment.Left, binary.Left) == true) {
					switch (binary.Operator) {
						case BinaryOperatorType.Add:
							assignment.Operator = AssignmentOperatorType.Add;
							break;
						case BinaryOperatorType.Subtract:
							assignment.Operator = AssignmentOperatorType.Subtract;
							break;
						case BinaryOperatorType.Multiply:
							assignment.Operator = AssignmentOperatorType.Multiply;
							break;
						case BinaryOperatorType.Divide:
							assignment.Operator = AssignmentOperatorType.Divide;
							break;
						case BinaryOperatorType.Modulus:
							assignment.Operator = AssignmentOperatorType.Modulus;
							break;
						case BinaryOperatorType.ShiftLeft:
							assignment.Operator = AssignmentOperatorType.ShiftLeft;
							break;
						case BinaryOperatorType.ShiftRight:
							assignment.Operator = AssignmentOperatorType.ShiftRight;
							break;
						case BinaryOperatorType.BitwiseAnd:
							assignment.Operator = AssignmentOperatorType.BitwiseAnd;
							break;
						case BinaryOperatorType.BitwiseOr:
							assignment.Operator = AssignmentOperatorType.BitwiseOr;
							break;
						case BinaryOperatorType.ExclusiveOr:
							assignment.Operator = AssignmentOperatorType.ExclusiveOr;
							break;
					}
					if (assignment.Operator != AssignmentOperatorType.Assign) {
						// If we found a shorter operators, get rid of the BinaryOperatorExpression:
						assignment.Right = binary.Right;
					}
				}
			}
			return null;
		}
		
		bool IsWithoutSideEffects(Expression left)
		{
			return left is IdentifierExpression; // TODO
		}
		
		/*
		public override object VisitCastExpression(CastExpression castExpression, object data)
		{
			MethodReference typeRef = invocationExpression.Annotation<TypeReference>();
			if (typeRef.FullName == Constants.Int32 &&
			    castExpression.Expression is MemberReferenceExpression &&
			    (castExpression.Expression as MemberReferenceExpression).MemberName == "Length") {
				ReplaceCurrentNode(castExpression.Expression);
				return null;
			}
			return base.VisitCastExpression(castExpression, data);
		}*/
	}
}
