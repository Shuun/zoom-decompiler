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

            internal void WriteChunk(ChunkWriter typeWriter)
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

            foreach (var t in asm.MainModule.Types)
            {
                var typeWriter = new ChunkWriter();

                AddType(t, typeWriter);

                assemblyChunk.WriteChunk(typeWriter);
            }

            assemblyChunkList.Add(assemblyChunk);
        }

        private void AddType(TypeDefinition t, ChunkWriter typeWriter)
        {
            throw new NotImplementedException();
        }

        void WriteToStream(Stream outputStream)
        {

            throw new NotImplementedException();
        }
    }
}