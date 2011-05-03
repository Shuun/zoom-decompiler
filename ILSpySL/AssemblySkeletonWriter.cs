using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;

using Mono.Cecil;

namespace ILSpySL
{
    public sealed class AssemblySkeletonWriter
    {
        private sealed class ChunkWriter
        {
            internal void WriteAssemblyName(AssemblyNameDefinition assemblyNameDefinition)
            {
                throw new NotImplementedException();
            }

            internal void WriteAttributes(IEnumerable<CustomAttribute> collection)
            {
                throw new NotImplementedException();
            }

            internal void WriteTypeName(TypeDefinition t)
            {
                throw new NotImplementedException();
            }

            internal void WriteInt32(int p)
            {
                throw new NotImplementedException();
            }

            internal void WriteTypeReference(TypeReference typeReference)
            {
                throw new NotImplementedException();
            }

            internal void WriteString(string p)
            {
                throw new NotImplementedException();
            }

            internal void WriteMethodReference(MethodReference methodDefinition)
            {
                throw new NotImplementedException();
            }

            internal void WriteBytes(byte[] bytes)
            {
                throw new NotImplementedException();
            }
        }

        readonly List<ChunkWriter> assemblyChunkList = new List<ChunkWriter>();

        private AssemblySkeletonWriter()
        {
        }

        public static void WriteSkeletons(IEnumerable<AssemblyDefinition> assemblies, Stream outputStream)
        {
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            var writer = new AssemblySkeletonWriter();
            
            foreach (var a in assemblies)
            {
                writer.AddAssembly(a);
            }

            writer.WriteToStream(outputStream);
        }

        void AddAssembly(AssemblyDefinition asm)
        {
            var assemblyChunk = new ChunkWriter();

            assemblyChunk.WriteAssemblyName(asm.Name);
            assemblyChunk.WriteAttributes(asm.CustomAttributes);

            assemblyChunk.WriteInt32(asm.MainModule.Types.Count);

            foreach (var t in asm.MainModule.Types)
            {
                WriteType(t, assemblyChunk);
            }

            assemblyChunkList.Add(assemblyChunk);
        }

        private void WriteType(TypeDefinition t, ChunkWriter typeWriter)
        {
            typeWriter.WriteTypeName(t);
            typeWriter.WriteInt32((int)t.Attributes);
            typeWriter.WriteAttributes(t.CustomAttributes);

            typeWriter.WriteTypeReference(t.BaseType);
            typeWriter.WriteTypeReference(t.DeclaringType);

            typeWriter.WriteInt32(t.Fields.Count);
            foreach (var f in t.Fields)
            {
                WriteField(f, typeWriter);
            }

            typeWriter.WriteInt32(t.Methods.Count);
            foreach (var m in t.Methods)
            {
                WriteMethod(m, typeWriter);
            }

            typeWriter.WriteInt32(t.Properties.Count);
            foreach (var p in t.Properties)
            {
                WriteProperty(p, typeWriter);
            }

            typeWriter.WriteInt32(t.Events.Count);
            foreach (var e in t.Events)
            {
                WriteEvent(e, typeWriter);
            }
        }

        private void WriteField(FieldDefinition f, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(f.Name);
            typeWriter.WriteInt32((int)f.Attributes);
            typeWriter.WriteAttributes(f.CustomAttributes);
            typeWriter.WriteBytes(f.InitialValue);
        }

        private void WriteEvent(EventDefinition e, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(e.Name);
            typeWriter.WriteInt32((int)e.Attributes);
            typeWriter.WriteAttributes(e.CustomAttributes);
            typeWriter.WriteMethodReference(e.AddMethod);
            typeWriter.WriteMethodReference(e.RemoveMethod);
        }

        private void WriteProperty(PropertyDefinition p, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(p.Name);
            typeWriter.WriteInt32((int)p.Attributes);
            typeWriter.WriteAttributes(p.CustomAttributes);
            typeWriter.WriteMethodReference(p.GetMethod);
            typeWriter.WriteMethodReference(p.SetMethod);
        }

        private void WriteMethod(MethodDefinition m, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(m.Name);
            typeWriter.WriteInt32((int)m.Attributes);
            typeWriter.WriteAttributes(m.CustomAttributes);
            typeWriter.WriteTypeReference(m.ReturnType);
            typeWriter.WriteInt32(m.Parameters.Count);
            foreach (var p in m.Parameters)
            {
                typeWriter.WriteString(p.Name);
                typeWriter.WriteInt32((int)p.Attributes);
                typeWriter.WriteAttributes(p.CustomAttributes);
                typeWriter.WriteTypeReference(p.ParameterType);
            }
        }

        void WriteToStream(Stream outputStream)
        {

            throw new NotImplementedException();
        }
    }
}