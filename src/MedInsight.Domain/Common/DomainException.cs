namespace MedInsight.Domain.Common;

/// <summary>
/// İş kuralı ihlali — API katmanında 409 Conflict'e eşlenir
/// (bkz. docs/backend/error-handling.md).
/// </summary>
public class DomainException(string message) : Exception(message);
