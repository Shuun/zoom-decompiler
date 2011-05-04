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
            private sealed class Usage<T>
            {
                private sealed class EqualityComparer : IEqualityComparer<T>
                {
                    readonly Func<T, object> key;

                    public EqualityComparer(Func<T,object> key)
                    {
                        this.key = key;
                    }

                    public bool Equals(T x, T y)
                    {
                        return Equals(key(x), key(y));
                    }

                    public int GetHashCode(T obj)
                    {
                        var k = key(obj);
                        if (k == null)
                            return 0;
                        else
                            return k.GetHashCode();
                    }
                }

                readonly Dictionary<T, int> counts;
                Dictionary<T, int> index;
                List<T> indexOrder;

                public Usage(Func<T, object> key)
                {
                    counts = new Dictionary<T, int>(new EqualityComparer(key));
                }

                public void Add(T item)
                {
                    if (index != null)
                        throw new InvalidOperationException("Index is already built, adding is not allowed any more.");

                    int count;
                    counts.TryGetValue(item, out count);
                    counts[item] = count + 1;
                }

                public int this[T item]
                {
                    get { return index[item]; }
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return indexOrder.GetEnumerator();
                }

                public void BuildIndex()
                {

                }
            }

            readonly List<Action> atoms = new List<Action>();

            readonly Usage<AssemblyNameDefinition> asmNames = new Usage<AssemblyNameDefinition>(a => a.FullName);
            readonly Usage<TypeReference> typeRefs = new Usage<TypeReference>(t => t.Module.Assembly.FullName+"\n"+t.FullName);
            readonly Usage<MethodReference> methodRefs = new Usage<MethodReference>(m => m.Module.Assembly.FullName + "\n" + m.DeclaringType.FullName+"\n"+m.FullName);
            readonly Usage<string> stringRefs = new Usage<string>(s => s);

            internal void WriteAssemblyName(AssemblyNameDefinition assemblyNameDefinition)
            {
                asmNames.Add(assemblyNameDefinition);

                atoms.Add(() => StreamWritePackedNumber(asmNames[assemblyNameDefinition]));
            }

            internal void WriteAttributes(IEnumerable<CustomAttribute> attributes)
            {
                WriteNumber(attributes == null ? 0 : attributes.Count());
                if (attributes != null)
                {
                    foreach (var attr in attributes)
                    {
                        WriteMethodReference(attr.Constructor);
                        WriteNumber(attr.ConstructorArguments.Count);
                        
                        foreach (var ctorArg in attr.ConstructorArguments)
                        {
                            WriteTypeReference(ctorArg.Type);
                            WriteConstant(ctorArg.Value);
                        }

                        WriteNumber(attr.Fields.Count);
                        foreach (var namedArg in attr.Fields)
                        {
                            WriteString(namedArg.Name);
                            WriteTypeReference(namedArg.Argument.Type);
                            WriteConstant(namedArg.Argument.Value);
                        }
                    }
                }
            }

            private void WriteConstant(object p)
            {
                throw new NotImplementedException();
            }

            internal void WriteTypeName(TypeReference t)
            {
                WriteString(t.Namespace);
                WriteString(t.Name);
            }

            internal void WriteNumber(int number)
            {
                this.atoms.Add(() => StreamWritePackedNumber(number));
            }

            internal void WriteTypeReference(TypeReference typeReference)
            {
                typeRefs.Add(typeReference);

                this.atoms.Add(() => StreamWritePackedNumber(typeRefs[typeReference]));
            }

            internal void WriteString(string str)
            {
                stringRefs.Add(str);

                this.atoms.Add(() => StreamWritePackedNumber(stringRefs[str]));
            }

            internal void WriteMethodReference(MethodReference methodReference)
            {
                methodRefs.Add(methodReference);

                this.atoms.Add(() => StreamWritePackedNumber(methodRefs[methodReference]));
            }

            internal void WriteBytes(byte[] bytes)
            {
                this.atoms.Add(() => this.StreamWriteBytes(bytes));
            }

            private void StreamWriteBytes(byte[] bytes)
            {
                throw new NotImplementedException();
            }

            private void StreamWritePackedNumber(int number)
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

            assemblyChunk.WriteNumber(asm.MainModule.Types.Count);

            foreach (var t in asm.MainModule.Types)
            {
                WriteType(t, assemblyChunk);
            }

            assemblyChunkList.Add(assemblyChunk);
        }

        private void WriteType(TypeDefinition t, ChunkWriter typeWriter)
        {
            typeWriter.WriteTypeName(t);
            typeWriter.WriteNumber((int)t.Attributes);
            typeWriter.WriteAttributes(t.CustomAttributes);

            typeWriter.WriteTypeReference(t.BaseType);
            typeWriter.WriteTypeReference(t.DeclaringType);

            typeWriter.WriteNumber(t.Fields.Count);
            foreach (var f in t.Fields)
            {
                WriteField(f, typeWriter);
            }

            typeWriter.WriteNumber(t.Methods.Count);
            foreach (var m in t.Methods)
            {
                WriteMethod(m, typeWriter);
            }

            typeWriter.WriteNumber(t.Properties.Count);
            foreach (var p in t.Properties)
            {
                WriteProperty(p, typeWriter);
            }

            typeWriter.WriteNumber(t.Events.Count);
            foreach (var e in t.Events)
            {
                WriteEvent(e, typeWriter);
            }
        }

        private void WriteField(FieldDefinition f, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(f.Name);
            typeWriter.WriteNumber((int)f.Attributes);
            typeWriter.WriteAttributes(f.CustomAttributes);
            typeWriter.WriteBytes(f.InitialValue);
        }

        private void WriteEvent(EventDefinition e, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(e.Name);
            typeWriter.WriteNumber((int)e.Attributes);
            typeWriter.WriteAttributes(e.CustomAttributes);
            typeWriter.WriteMethodReference(e.AddMethod);
            typeWriter.WriteMethodReference(e.RemoveMethod);
        }

        private void WriteProperty(PropertyDefinition p, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(p.Name);
            typeWriter.WriteNumber((int)p.Attributes);
            typeWriter.WriteAttributes(p.CustomAttributes);
            typeWriter.WriteMethodReference(p.GetMethod);
            typeWriter.WriteMethodReference(p.SetMethod);
        }

        private void WriteMethod(MethodDefinition m, ChunkWriter typeWriter)
        {
            typeWriter.WriteString(m.Name);
            typeWriter.WriteNumber((int)m.Attributes);
            typeWriter.WriteAttributes(m.CustomAttributes);
            typeWriter.WriteTypeReference(m.ReturnType);
            typeWriter.WriteNumber(m.Parameters.Count);
            foreach (var p in m.Parameters)
            {
                typeWriter.WriteString(p.Name);
                typeWriter.WriteNumber((int)p.Attributes);
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