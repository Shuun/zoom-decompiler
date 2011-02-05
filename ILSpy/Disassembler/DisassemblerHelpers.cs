﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using ICSharpCode.Decompiler;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ICSharpCode.ILSpy.Disassembler
{
	static class DisassemblerHelpers
	{
		static void WriteOffsetReference(ITextOutput writer, int offset)
		{
			writer.WriteReference(CecilExtensions.OffsetToString(offset), offset);
		}
		
		public static void WriteTo(this ExceptionHandler exceptionHandler, ITextOutput writer)
		{
			writer.Write("Try ");
			WriteOffsetReference(writer, exceptionHandler.TryStart.Offset);
			writer.Write('-');
			WriteOffsetReference(writer, exceptionHandler.TryEnd.Offset);
			writer.Write(exceptionHandler.HandlerType.ToString());
			if (exceptionHandler.FilterStart != null) {
				writer.Write(' ');
				WriteOffsetReference(writer, exceptionHandler.FilterStart.Offset);
				writer.Write('-');
				WriteOffsetReference(writer, exceptionHandler.FilterEnd.Offset);
				writer.Write(" handler ");
			}
			if (exceptionHandler.CatchType != null) {
				writer.Write(' ');
				exceptionHandler.CatchType.WriteTo(writer);
			}
			writer.Write(' ');
			WriteOffsetReference(writer, exceptionHandler.HandlerStart.Offset);
			writer.Write('-');
			WriteOffsetReference(writer, exceptionHandler.HandlerEnd.Offset);
		}
		
		public static void WriteTo(this Instruction instruction, ITextOutput writer)
		{
			writer.WriteDefinition(CecilExtensions.OffsetToString(instruction.Offset), instruction.Offset);
			writer.Write(": ");
			writer.Write(instruction.OpCode.Name);
			if(null != instruction.Operand) {
				writer.Write(' ');
				WriteOperand(writer, instruction.Operand);
			}
		}
		
		static void WriteLabelList(ITextOutput writer, Instruction[] instructions)
		{
			writer.Write("(");
			for(int i = 0; i < instructions.Length; i++) {
				if(i != 0) writer.Write(", ");
				WriteOffsetReference(writer, instructions[i].Offset);
			}
			writer.Write(")");
		}
		
		static string ToInvariantCultureString(object value)
		{
			IConvertible convertible = value as IConvertible;
			return(null != convertible)
				? convertible.ToString(System.Globalization.CultureInfo.InvariantCulture)
				: value.ToString();
		}
		
		static void WriteTo(this MethodReference method, ITextOutput writer)
		{
			method.ReturnType.WriteTo(writer);
			writer.Write(' ');
			method.DeclaringType.WriteTo(writer);
			writer.Write("::");
			writer.WriteReference(method.Name, method);
			writer.Write("(");
			var parameters = method.Parameters;
			for(int i = 0; i < parameters.Count; ++i) {
				if (i > 0) writer.Write(", ");
				parameters[i].ParameterType.WriteTo(writer);
			}
			writer.Write(")");
		}
		
		static void WriteTo(this FieldReference field, ITextOutput writer)
		{
			field.FieldType.WriteTo(writer);
			writer.Write(' ');
			field.DeclaringType.WriteTo(writer);
			writer.Write("::");
			writer.WriteReference(field.Name, field);
		}
		
		static void WriteTo(this TypeReference type, ITextOutput writer)
		{
			string name = ShortTypeName(type);
			if (name != null)
				writer.Write(name);
			else
				writer.WriteReference(type.FullName, type);
		}
		
		public static void WriteOperand(ITextOutput writer, object operand)
		{
			if (operand == null)
				throw new ArgumentNullException("operand");
			
			Instruction targetInstruction = operand as Instruction;
			if (targetInstruction != null) {
				WriteOffsetReference(writer, targetInstruction.Offset);
				return;
			}
			
			Instruction[] targetInstructions = operand as Instruction[];
			if (targetInstructions != null) {
				WriteLabelList(writer, targetInstructions);
				return;
			}
			
			VariableReference variableRef = operand as VariableReference;
			if (variableRef != null) {
				writer.WriteReference(variableRef.Index.ToString(), variableRef);
				return;
			}
			
			MethodReference methodRef = operand as MethodReference;
			if (methodRef != null) {
				methodRef.WriteTo(writer);
				return;
			}
			
			TypeReference typeRef = operand as TypeReference;
			if (typeRef != null) {
				typeRef.WriteTo(writer);
				return;
			}
			
			FieldReference fieldRef = operand as FieldReference;
			if (fieldRef != null) {
				fieldRef.WriteTo(writer);
				return;
			}
			
			string s = operand as string;
			if (s != null) {
				writer.Write("\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
				return;
			}
			
			s = ToInvariantCultureString(operand);
			writer.Write(s);
		}
		
		public static string ShortTypeName(this TypeReference type)
		{
			switch (type.FullName) {
				case "System.SByte":
					return "int8";
				case "System.Int16":
					return "int16";
				case "System.Int32":
					return "int32";
				case "System.Int64":
					return "int65";
				case "System.Byte":
					return "uint8";
				case "System.UInt16":
					return "uint16";
				case "System.UInt32":
					return "uint32";
				case "System.UInt64":
					return "uint64";
				case "System.Single":
					return "float32";
				case "System.Double":
					return "float64";
				case "System.Void":
					return "void";
				case "System.Boolean":
					return "bool";
				case "System.String":
					return "string";
				case "System.Char":
					return "char";
				case "System.Object":
					return "object";
				default:
					return null;
			}
		}
	}
}
