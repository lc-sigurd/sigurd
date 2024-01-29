using System;
using Sigurd.Common.Resources;

namespace Sigurd.Common;

/// <summary>
/// Represents <see cref="ResourceLocation"/> errors that occur during application execution.
/// </summary>
public class ResourceLocationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceLocationException"/> class.
    /// </summary>
    public ResourceLocationException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceLocationException"/> class with a
    /// specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ResourceLocationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceLocationException"/> class with a
    /// specified error message and a reference to the inner exception that is the cause of
    /// this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="inner">
    /// The exception that is the cause of the current exception, or a null reference
    /// (Nothing in Visual Basic) if no inner exception is specified.
    /// </param>
    public ResourceLocationException(string message, Exception inner) : base(message, inner) { }
}
