using System;

namespace FanHubPlus.Core.Exceptions;

/// <summary>
/// Base exception for all domain-specific exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }
    public object? AdditionalData { get; }

    protected DomainException(string message, string errorCode, object? additionalData = null)
        : base(message)
    {
        ErrorCode = errorCode;
        AdditionalData = additionalData;
    }
}

/// <summary>
/// Thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was not found", "NOT_FOUND", new { EntityName = entityName, Id = id })
    {
    }

    public NotFoundException(string message)
        : base(message, "NOT_FOUND")
    {
    }
}

/// <summary>
/// Thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION", object? additionalData = null)
        : base(message, errorCode, additionalData)
    {
    }
}

/// <summary>
/// Thrown when a concurrency conflict occurs (optimistic locking).
/// </summary>
public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string entityName, object id)
        : base($"{entityName} with ID '{id}' was modified by another user", "CONCURRENCY_CONFLICT", new { EntityName = entityName, Id = id })
    {
    }
}

/// <summary>
/// Thrown when validation fails at the domain level.
/// </summary>
public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message, "VALIDATION_FAILED", errors)
    {
        Errors = errors;
    }

    public ValidationException(string message)
        : base(message, "VALIDATION_FAILED", new Dictionary<string, string[]>())
    {
        Errors = new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Thrown when an external service is unavailable.
/// </summary>
public class ExternalServiceException : DomainException
{
    public string ServiceName { get; }
    public int? StatusCode { get; }

    public ExternalServiceException(string serviceName, string message, int? statusCode = null, Exception? innerException = null)
        : base(message, "EXTERNAL_SERVICE_ERROR", new { ServiceName = serviceName, StatusCode = statusCode })
    {
        ServiceName = serviceName;
        StatusCode = statusCode;

        if (innerException is not null)
        {
            // Can't set inner exception after base constructor, but we log it
        }
    }
}

/// <summary>
/// Thrown when a game cannot be loaded or played.
/// </summary>
public class GameLoadException : DomainException
{
    public int GameId { get; }
    public GameLoadReason Reason { get; }

    public GameLoadException(int gameId, GameLoadReason reason, string message)
        : base(message, "GAME_LOAD_FAILED", new { GameId = gameId, Reason = reason })
    {
        GameId = gameId;
        Reason = reason;
    }
}

public enum GameLoadReason
{
    NotFound,
    Unauthorized,
    InvalidFormat,
    MissingFiles,
    SandboxViolation,
    ExternalServiceUnavailable,
    CloudSaveConflict
}