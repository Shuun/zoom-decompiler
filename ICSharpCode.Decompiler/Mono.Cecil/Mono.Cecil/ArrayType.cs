//
// ArrayType.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Copyright (c) 2008 - 2011 Jb Evain
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using Mono.Collections.Generic;
using MD = Mono.Cecil.Metadata;
using System.Collections.Generic;

using Mi.Decompiler;

namespace Mono.Cecil
{
    public struct ArrayDimension
    {
        readonly int m_LowerBound;
        readonly int m_UpperBoundIncremented;

        public ArrayDimension(int? lowerBound, int? upperBound)
        {
            this.m_LowerBound = lowerBound ?? -1;
            this.m_UpperBoundIncremented = (upperBound ?? -1) + 1;
        }

        public int? LowerBound { get { return m_LowerBound == 0 ? (int?)null : m_LowerBound; } }
        public int? UpperBound { get { return m_UpperBoundIncremented == 0 ? (int?)null : m_UpperBoundIncremented - 1; } }

        public bool IsSized { get { return m_LowerBound > 0 || m_UpperBoundIncremented != 0; } }

        public override string ToString()
        {
            return IsSized ? this.LowerBound + "..." + this.UpperBound : "~";
        }
    }

    public sealed class ArrayType : TypeSpecification
    {
        static readonly System.Collections.ObjectModel.ReadOnlyCollection<ArrayDimension> VectorDimension = new System.Collections.ObjectModel.ReadOnlyCollection<ArrayDimension>(
            new[] { new ArrayDimension(0, null) });

        readonly System.Collections.ObjectModel.ReadOnlyCollection<ArrayDimension> m_Dimensions;

        public ArrayType(TypeReference type)
            : base(type)
        {
            Mixin.CheckType(type);
            this.etype = MD.ElementType.Array;
        }

        public ArrayType(TypeReference type, int rank)
            : base(type)
        {
            Mixin.CheckType(type);

            if (rank == 1)
                return;

            m_Dimensions = new System.Collections.ObjectModel.ReadOnlyCollection<ArrayDimension>(new ArrayDimension[rank]);

            this.etype = MD.ElementType.Array;
        }

        public ArrayType(TypeReference type, IEnumerable<ArrayDimension> dimensions)
            : base(type)
        {
            Mixin.CheckType(type);
            m_Dimensions = dimensions.ToReadOnlyCollectionOrNull();

            if (m_Dimensions.Count == 1
                && !m_Dimensions[0].IsSized)
                m_Dimensions = null;

            this.etype = MD.ElementType.Array;
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<ArrayDimension> Dimensions { get { return m_Dimensions ?? VectorDimension; } }
        public int Rank { get { return this.Dimensions.Count; } }
        public bool IsVector { get { return m_Dimensions == null; } }

        public override bool IsValueType
        {
            get { return false; }
            set { throw new InvalidOperationException(); }
        }

        public override string Name
        {
            get { return base.Name + Suffix; }
        }

        public override string FullName
        {
            get { return base.FullName + Suffix; }
        }

        public override bool IsArray
        {
            get { return true; }
        }

        string Suffix
        {
            get
            {
                if (IsVector)
                    return "[]";

                var suffix = new StringBuilder();
                suffix.Append("[");
                for (int i = 0; i < m_Dimensions.Count; i++)
                {
                    if (i > 0)
                        suffix.Append(",");

                    suffix.Append(m_Dimensions[i].ToString());
                }
                suffix.Append("]");

                return suffix.ToString();
            }
        }

    }
}