
using System;
using System.Runtime.Serialization;

namespace Mi.NRefactory.TypeSystem
{
	/// <summary>
	/// Represents an error while parsing a reflection name.
	/// </summary>
	public class ReflectionNameParseException : Exception
	{
		int position;
		
		public int Position {
			get { return position; }
		}
		
		public ReflectionNameParseException(int position)
		{
			this.position = position;
		}
		
		public ReflectionNameParseException(int position, string message) : base(message)
		{
			this.position = position;
		}
		
		public ReflectionNameParseException(int position, string message, Exception innerException) : base(message, innerException)
		{
			this.position = position;
		}
	}
}