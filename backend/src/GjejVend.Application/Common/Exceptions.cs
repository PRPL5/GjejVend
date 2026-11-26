namespace GjejVend.Application.Common;

/// <summary>The requested record does not exist. Mapped to HTTP 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>The caller is not allowed to do this. Mapped to HTTP 403.</summary>
public sealed class ForbiddenException(string message) : Exception(message);

/// <summary>The request is valid HTTP but breaks a business rule. Mapped to HTTP 400.</summary>
public sealed class BusinessRuleException(string message) : Exception(message);
