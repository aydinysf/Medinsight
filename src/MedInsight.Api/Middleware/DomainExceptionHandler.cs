using MedInsight.Application.Common;
using MedInsight.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MedInsight.Api.Middleware;

/// <summary>
/// Domain hatası → 409, yetki ihlali → 403, doğrulama benzeri argüman hatası → 400
/// (bkz. docs/backend/error-handling.md). Teknik hatalar 500 olarak akmaya devam eder.
/// </summary>
public sealed class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status409Conflict, "İş kuralı ihlali"),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, "Erişim engellendi"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Geçersiz istek"),
            _ => (0, string.Empty),
        };

        if (statusCode == 0)
        {
            return false;
        }

        logger.LogWarning(exception, "Domain hatası: {Message} (correlationId: {CorrelationId})", exception.Message, httpContext.TraceIdentifier);

        await Results.Problem(
                title: title,
                detail: exception.Message,
                statusCode: statusCode,
                extensions: new Dictionary<string, object?> { ["correlationId"] = httpContext.TraceIdentifier })
            .ExecuteAsync(httpContext);

        return true;
    }
}
