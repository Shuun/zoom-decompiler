using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.Decompiler.Tests.Helpers
{
    public sealed class MatchException : Exception
    {
        public readonly string OriginalCode;
        public readonly string DecompiledCode;
        public readonly string DiffSummary;

        public MatchException(string originalCode, string decompiledCode, string diffSummary)
            : base("Decompilation does not match.")
        {
            this.OriginalCode = originalCode;
            this.DecompiledCode = decompiledCode;
            this.DiffSummary = diffSummary;
        }
    }
}
