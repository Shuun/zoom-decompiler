using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using System.IO;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.FormattingTests
{
	/// <summary>
	/// Text editor test adapter. Only implemented for testing purposes. Don't use in production code.
	/// </summary>
	class TextEditorTestAdapter : ITextEditorAdapter
	{
		string text;

		public string Text {
			get {
				return this.text;
			}
		}

		List<Delimiter> delimiters;
		
		struct Delimiter
		{
			public readonly int Offset;
			public readonly int Length;

			public int EndOffset {
				get { return Offset + Length; }
			}

			public Delimiter (int offset, int length)
			{
				Offset = offset;
				Length = length;
			}
			
			public override string ToString ()
			{
				return string.Format ("[Delimiter: Offset={0}, Length={1}]", Offset, Length);
			}
		}

		static IEnumerable<Delimiter> FindDelimiter (string text)
		{
			for (int i = 0; i < text.Length; i++) {
				switch (text [i]) {
				case '\r':
					if (i + 1 < text.Length && text [i + 1] == '\n') {
						yield return new Delimiter (i, 2);
						i++;
					} else {
						yield return new Delimiter (i, 1);
					}
					break;
				case '\n':
					yield return new Delimiter (i, 1);
					break;
				}
			}
		}

		public TextEditorTestAdapter (string text)
		{
			this.text = text;
			delimiters = new  List<Delimiter> (FindDelimiter (text));
		}
		
		class Segment
		{
			public readonly int Offset;
			public readonly int Length;
			public readonly int DelimiterLength;
			
			public Segment (int offset, int length, int delimiterLength)
			{
				this.Offset = offset;
				this.Length = length;
				this.DelimiterLength = delimiterLength;
			}
			
			public override string ToString ()
			{
				return string.Format ("[Segment: Offset={0}, Length={1}, DelimiterLength={2}]", Offset, Length, DelimiterLength);
			}
		}
		
		Segment Get (int number)
		{
			number--;
			if (number < 0 || number - 1 >= delimiters.Count)
				return null;
			int startOffset = number > 0 ? delimiters [number - 1].EndOffset : 0;
			int endOffset;
			int delimiterLength;
			if (number < delimiters.Count) {
				endOffset = delimiters [number].EndOffset;
				delimiterLength = delimiters [number].Length;
			} else {
				endOffset = text.Length;	
				delimiterLength = 0;
			}
			return new Segment (startOffset, endOffset - startOffset, delimiterLength);
		}
		
		public void ApplyChanges (List<Change> changes)
		{
			changes.Sort ((x, y) => x.Offset.CompareTo (y.Offset));
			changes.Reverse ();
			foreach (var change in changes) {
				text = text.Substring (0, change.Offset) + change.InsertedText + text.Substring (change.Offset + change.RemovedChars);
			}
			delimiters = new  List<Delimiter> (FindDelimiter (text));
		}
		
		#region ITextEditorAdapter implementation
		public int LocationToOffset (int line, int col)
		{
			Segment seg = Get (line);
			if (seg == null)
				return 0;
			return seg.Offset + col - 1;
		}

		public char GetCharAt (int offset)
		{
			if (offset < 0 || offset >= text.Length)
				return '\0';
			return text [offset];
		}

		public string GetTextAt (int offset, int length)
		{
			if (offset < 0 || offset + length >= text.Length)
				return  "";
			if (length <= 0)
				return "";
			
			return text.Substring (offset, length);
		}

		public int GetEditableLength (int lineNumber)
		{
			var seg = Get (lineNumber);
			if (seg == null)
				return 0;
			return seg.Length - seg.DelimiterLength;
		}

		public string GetIndentation (int lineNumber)
		{
			var seg = Get (lineNumber);
			if (seg == null)
				return "";
			int start = seg.Offset;
			int end = seg.Offset;
			int endOffset = seg.Offset + seg.Length - seg.DelimiterLength;
			
			while (end < endOffset && (text[end] == ' ' || text[end] == '\t'))
				end++;
			
			return start < end ? text.Substring (start, end - start) : "";
		}

		public int GetLineOffset (int lineNumber)
		{
			var seg = Get (lineNumber);
			if (seg == null)
				return 0;
			return seg.Offset;
		}

		public int GetLineLength (int lineNumber)
		{
			var seg = Get (lineNumber);
			if (seg == null)
				return 0;
			return seg.Length;
		}

		public int GetLineEndOffset (int lineNumber)
		{
			var seg = Get (lineNumber);
			if (seg == null)
				return 0;
			return seg.Offset + seg.Length;
		}

		public bool TabsToSpaces {
			get {
				return false;
			}
		}

		public int TabSize {
			get {
				return 4;
			}
		}

		public string EolMarker {
			get {
				return Environment.NewLine;
			}
		}

		public int Length {
			get {
				return text.Length;
			}
		}

		public int LineCount {
			get {
				return delimiters.Count + 1;
			}
		}
		#endregion
	}
	
	public abstract class TestBase
	{
		protected static ITextEditorAdapter GetResult (CSharpFormattingPolicy policy, string input)
		{
			var adapter = new TextEditorTestAdapter (input);
			var visitior = new AstFormattingVisitor (policy, adapter);
			
			var compilationUnit = new CSharpParser ().Parse (new StringReader (adapter.Text));
			compilationUnit.AcceptVisitor (visitior, null);
			adapter.ApplyChanges (visitior.Changes);

			return adapter;
		}
		
		protected static ITextEditorAdapter Test (CSharpFormattingPolicy policy, string input, string expectedOutput)
		{
			var adapter = new TextEditorTestAdapter (input);
			var visitior = new AstFormattingVisitor (policy, adapter);
			
			var compilationUnit = new CSharpParser ().Parse (new StringReader (adapter.Text));
			compilationUnit.AcceptVisitor (visitior, null);
			adapter.ApplyChanges (visitior.Changes);
			Assert.AreEqual (expectedOutput, adapter.Text);
			return adapter;
		}

		protected static void Continue (CSharpFormattingPolicy policy, ITextEditorAdapter adapter, string expectedOutput)
		{
			var visitior = new AstFormattingVisitor (policy, adapter);
			
			var compilationUnit = new CSharpParser ().Parse (new StringReader (adapter.Text));
			compilationUnit.AcceptVisitor (visitior, null);
			((TextEditorTestAdapter)adapter).ApplyChanges (visitior.Changes);
			Assert.AreEqual (expectedOutput, adapter.Text);
		}

		
	}
}

