﻿// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

#if !SILVERLIGHT

using System.Runtime.Serialization;
using Microsoft.Internal.Runtime.Serialization;

#endif

namespace System.ComponentModel.Composition
{
    /// <summary>
    ///     The exception that is thrown when one or more errors occur during composition in 
    ///     a <see cref="CompositionContainer"/>.
    /// </summary>
    [Serializable]
    public class CompositionException : Exception
    {
        private readonly ReadOnlyCollection<CompositionError> _errors;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionException"/> class.
        /// </summary>
        public CompositionException()
            : this((string)null, (Exception)null, (IEnumerable<CompositionError>)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionException"/> class 
        ///     with the specified error message.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="String"/> containing a message that describes the 
        ///     <see cref="CompositionException"/>; or <see langword="null"/> to set
        ///     the <see cref="Exception.Message"/> property to its default value.
        /// </param>
        public CompositionException(string message)
            : this(message, (Exception)null, (IEnumerable<CompositionError>)null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionException"/> class 
        ///     with the specified error message and exception that is the cause of the  
        ///     exception.
        /// </summary>
        /// <param name="message">
        ///     A <see cref="String"/> containing a message that describes the 
        ///     <see cref="CompositionException"/>; or <see langword="null"/> to set
        ///     the <see cref="Exception.Message"/> property to its default value.
        /// </param>
        /// <param name="innerException">
        ///     The <see cref="Exception"/> that is the underlying cause of the 
        ///     <see cref="ComposablePartException"/>; or <see langword="null"/> to set
        ///     the <see cref="Exception.InnerException"/> property to <see langword="null"/>.
        /// </param>
        public CompositionException(string message, Exception innerException)
            : this(message, innerException, (IEnumerable<CompositionError>)null)
        {
        }

        internal CompositionException(CompositionError error)
            : this(new CompositionError[] { error })
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionException"/> class 
        ///     with the specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An <see cref="IEnumerable{T}"/> of <see cref="CompositionError"/> objects
        ///     representing the errors that are the cause of the 
        ///     <see cref="CompositionException"/>; or <see langword="null"/> to set the 
        ///     <see cref="Errors"/> property to an empty <see cref="IEnumerable{T}"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="errors"/> contains an element that is <see langword="null"/>.
        /// </exception>
        public CompositionException(IEnumerable<CompositionError> errors)
            : this((string)null, (Exception)null, errors)
        {
        }

        internal CompositionException(string message, Exception innerException, IEnumerable<CompositionError> errors)
            : base(message, innerException)
        {
            Requires.NullOrNotNullElements(errors, "errors");

            this._errors = new ReadOnlyCollection<CompositionError>(errors == null ? new CompositionError[0] : errors.ToArray());
        }

#if !SILVERLIGHT

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositionException"/> class 
        ///     with the specified serialization data.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"/> that holds the serialized object data about the 
        ///     <see cref="CompositionException"/>.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"/> that contains contextual information about the 
        ///     source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="info"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="SerializationException">
        ///     <paramref name="info"/> is missing a required value.
        /// </exception>
        /// <exception cref="InvalidCastException">
        ///     <paramref name="info"/> contains a value that cannot be cast to the correct type.
        /// </exception>
        [System.Security.SecuritySafeCritical]
        protected CompositionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this._errors = info.GetValue<ReadOnlyCollection<CompositionError>>("Errors");
        }

#endif //!SILVERLIGHT

        /// <summary>
        ///     Gets the errors that are the cause of the exception.
        /// </summary>
        /// <value>
        ///     An <see cref="IEnumerable{T}"/> of <see cref="CompositionError"/> objects
        ///     representing the errors that are the cause of the 
        ///     <see cref="CompositionException"/>.
        /// </value>
        public ReadOnlyCollection<CompositionError> Errors
        {
            get { return _errors; }
        }

        /// <summary>
        ///     Gets a message that describes the exception.
        /// </summary>
        /// <value>
        ///     A <see cref="String"/> containing a message that describes the 
        ///     <see cref="CompositionException"/>.
        /// </value>
        public override string Message
        {
            [System.Security.SecuritySafeCritical]
            get
            {
                if (this.Errors.Count == 0)
                {   // If there are no errors, then we simply return base.Message, 
                    // which will either use the default Exception message, or if 
                    // one was specified; the user supplied message.

                    return base.Message;
                }

                return BuildDefaultMessage();
            }
        }

#if !SILVERLIGHT

        /// <summary>
        ///     Gets the serialization data of the exception.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo"/> that holds the serialized object data about the 
        ///     <see cref="ComposablePartException"/>.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext"/> that contains contextual information about the 
        ///     source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="info"/> is <see langword="null"/>.
        /// </exception>
        [System.Security.SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Errors", _errors);
        }

#endif

        private string BuildDefaultMessage()
        {
            IEnumerable<IEnumerable<CompositionError>> paths = CalculatePaths(this);

            StringBuilder writer = new StringBuilder();

            WriteHeader(writer, this.Errors.Count, paths.Count());
            WritePaths(writer, paths);

            return writer.ToString();
        }

        private static void WriteHeader(StringBuilder writer, int errorsCount, int pathCount)
        {
            if (errorsCount > 1 && pathCount > 1)
            {
                // The composition produced multiple composition errors, with {0} root causes. The root causes are provided below.
                writer.AppendFormat(
                     CultureInfo.CurrentCulture,
                     Strings.CompositionException_MultipleErrorsWithMultiplePaths,
                     pathCount);
            }
            else if (errorsCount == 1 && pathCount > 1)
            {
                // The composition produced a single composition error, with {0} root causes. The root causes are provided below.
                writer.AppendFormat(
                     CultureInfo.CurrentCulture,
                     Strings.CompositionException_SingleErrorWithMultiplePaths,
                     pathCount);
            }
            else
            {
                Assumes.IsTrue(errorsCount == 1);
                Assumes.IsTrue(pathCount == 1);
                
                // The composition produced a single composition error. The root cause is provided below.
                writer.AppendFormat(
                     CultureInfo.CurrentCulture,
                     Strings.CompositionException_SingleErrorWithSinglePath,
                     pathCount);
            }

            writer.Append(' ');
            writer.AppendLine(Strings.CompositionException_ReviewErrorProperty);
        }

        private static void WritePaths(StringBuilder writer, IEnumerable<IEnumerable<CompositionError>> paths)
        {
            int ordinal = 0;
            foreach (IEnumerable<CompositionError> path in paths)
            {
                ordinal++;
                WritePath(writer, path, ordinal);
            }
        }

        private static void WritePath(StringBuilder writer, IEnumerable<CompositionError> path, int ordinal)
        {
            writer.AppendLine();
            writer.Append(ordinal.ToString(CultureInfo.CurrentCulture));
            writer.Append(") ");

            WriteError(writer, path.First());

            foreach (CompositionError error in path.Skip(1))
            {
                writer.AppendLine();
                writer.Append(Strings.CompositionException_ErrorPrefix);
                writer.Append(' ');
                WriteError(writer, error);
            }
        }

        private static void WriteError(StringBuilder writer, CompositionError error)
        {
            writer.AppendLine(error.Description);

            if (error.Element != null)
            {
                WriteElementGraph(writer, error.Element);
            }
        }

        private static void WriteElementGraph(StringBuilder writer, ICompositionElement element)
        {
            // Writes the composition element and its origins in the format:
            // Element: Export --> Part --> PartDefinition --> Catalog

            writer.AppendFormat(CultureInfo.CurrentCulture, Strings.CompositionException_ElementPrefix, element.DisplayName);

            while ((element = element.Origin) != null)
            {
                writer.AppendFormat(CultureInfo.CurrentCulture, Strings.CompositionException_OriginSeparator, element.DisplayName);
            }

            writer.AppendLine();
        }

        private static IEnumerable<IEnumerable<CompositionError>> CalculatePaths(CompositionException exception)
        {
            List<IEnumerable<CompositionError>> paths = new List<IEnumerable<CompositionError>>();

            VisitContext context = new VisitContext();
            context.Path = new Stack<CompositionError>();
            context.LeafVisitor = path =>
            {
                // Take a snapshot of the path
                paths.Add(path.Copy());
            };

            VisitCompositionException(exception, context);

            return paths;
        }

        private static void VisitCompositionException(CompositionException exception, VisitContext context)
        {
            foreach (CompositionError error in exception.Errors)
            {
                VisitError(error, context);
            }

            if (exception.InnerException != null)
            {
                VisitException(exception.InnerException, context);
            }
        }

        private static void VisitError(CompositionError error, VisitContext context)
        {
            context.Path.Push(error);

            if (error.Exception == null)
            {   // This error is a root cause, so write 
                // out the stack from this point

                context.LeafVisitor(context.Path);
            }
            else
            {
                VisitException(error.Exception, context);
            }

            context.Path.Pop();
        }

        private static void VisitException(Exception exception, VisitContext context)
        {
            CompositionException composition = exception as CompositionException;
            if (composition != null)
            {
                VisitCompositionException(composition, context);
            }
            else
            {
                VisitError(new CompositionError(exception.Message, exception.InnerException), context);
            }
        }

        private struct VisitContext
        {
            public Stack<CompositionError> Path;
            public Action<Stack<CompositionError>> LeafVisitor;
        }
    }
}
