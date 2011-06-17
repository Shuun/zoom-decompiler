using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using Mi.Decompiler;

namespace Mi.Zoom
{
    sealed class RichTextOutput : ITextOutput
    {
        readonly Brush KeywordBrush = new SolidColorBrush(Colors.DarkGray);
        readonly Brush IdentifierBrush = new SolidColorBrush(Colors.Blue);

        readonly List<Paragraph> blocks = new List<Paragraph>();
        int indentCount = 0;
        bool newLine = true;

        public int CurrentLine { get { return blocks.Count; } }
        public int CurrentColumn { get { return 0; } }

        public void Indent() { indentCount++; }
        public void Unindent() { indentCount--; }

        public void Write(char ch)
        {
            Write(ch.ToString());
        }

        public void WriteKeyword(string text)
        {
            CheckCompleteLine();

            this.blocks.Last().Inlines.Add(
                new Run { Text = text, Foreground = KeywordBrush });
        }

        public void WriteIdentifier(string text)
        {
            CheckCompleteLine();

            this.blocks.Last().Inlines.Add(
                new Run { Text = text, Foreground = IdentifierBrush, FontWeight = FontWeights.Bold });
        }

        public void Write(string text)
        {
            CheckCompleteLine();

            this.blocks.Last().Inlines.Add(
                new Run { Text = text });
        }

        public void WriteLine()
        {
            if (newLine)
                CheckCompleteLine();

            newLine = true;
        }

        public void WriteDefinition(string text, object definition)
        {
            CheckCompleteLine();

            this.blocks.Last().Inlines.Add(
                new Run { Text = text, Foreground = new SolidColorBrush(Colors.Blue) });
        }

        public void WriteReference(string text, object reference)
        {
            CheckCompleteLine();

            var bold = new Bold();
            bold.Inlines.Add(new Run { Text = text });
            this.blocks.Last().Inlines.Add(bold);
        }

        public void MarkFoldStart(string collapsedText = "...", bool defaultCollapsed = false)
        {
        }

        public void MarkFoldEnd()
        {
        }

        void CheckCompleteLine()
        {
            if (newLine)
            {
                var newPara = new Paragraph { };
                if (this.indentCount > 0)
                {
                    newPara.Inlines.Add(new Run { Text = new string('\t', this.indentCount) });
                }
                blocks.Add(newPara);
                newLine = false;
            }
        }

        public IEnumerable<Block> GetBlocks()
        {
            foreach (var b in this.blocks)
            {
                yield return b;
            }
        }
    }
}
