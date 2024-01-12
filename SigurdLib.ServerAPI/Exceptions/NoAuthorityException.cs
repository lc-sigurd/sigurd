namespace Sigurd.ServerAPI.Exceptions;

internal class NoAuthorityException : Exception
{
    internal NoAuthorityException(string message) : base(message) { }
}