namespace GjejVend.Domain.Common;

public abstract class DomainException : Exception
{
    protected DomainException(string code, string message, Exception? innerException = null)
        : base(message, innerException) => Code = code;

    public string Code { get; }
}

public sealed class ConflictException(string code, string message, Exception? innerException = null)
    : DomainException(code, message, innerException);

public sealed class NotFoundException(string code, string message)
    : DomainException(code, message);

public sealed class ForbiddenException(string code, string message)
    : DomainException(code, message);
