using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;

using Mono.Cecil;

namespace ILSpySL
{
    public sealed class AssemblySkeletonReader
    {
        readonly Stream inputStream;
        
        public AssemblySkeletonReader(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException("inputStream");
            if (!inputStream.CanRead)
                throw new ArgumentException("Reading is necessary but prohibited on this stream.", "inputStream");
            if(!inputStream.CanSeek)
                throw new ArgumentException("Random access (seek) is necessary but prohibited on this stream.", "inputStream");

            this.inputStream = inputStream;
        }

        public ReadOnlyCollection<string> Names { get { throw new NotImplementedException(); } }

        public AssemblyDefinition this[string name]
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}