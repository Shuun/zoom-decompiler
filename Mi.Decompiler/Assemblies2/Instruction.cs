using System;
using System.Collections.Generic;
using System.Linq;

namespace Mi.Assemblies2
{
    public abstract class Instruction
    {
        readonly SequencePoint m_SequencePoint;

        public Instruction(SequencePoint sequencePoint)
        {
            this.m_SequencePoint = sequencePoint;
        }

        public abstract System.Reflection.Emit.OpCode OpCode { get; }
    }
}