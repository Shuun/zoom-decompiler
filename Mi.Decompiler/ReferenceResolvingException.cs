using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mi.Decompiler
{
	/// <summary>
	/// Represents an error while resolving a reference to a type or a member.
	/// </summary>
	public class ReferenceResolvingException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:ResolveException"/> class
		/// </summary>
		public ReferenceResolvingException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ResolveException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the error. The content of message is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
		public ReferenceResolvingException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ResolveException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the error. The content of message is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
		/// <param name="inner">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
		public ReferenceResolvingException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
