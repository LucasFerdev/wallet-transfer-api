using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using WalletTransfer.Application.Common.Exceptions;
using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.Api.Common.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = MapException(httpContext, exception);

        if (error.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Erro não esperado ao processar a requisição.");
        }
        else
        {
            logger.LogWarning(
                exception,
                "Requisição rejeitada: {Message}",
                exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = error.Title,
            Detail = error.Detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = error.StatusCode;
        httpContext.Response.ContentType =
            "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ErrorDetails MapException(
        HttpContext httpContext,
        Exception exception)
    {
        return exception switch
        {
            UserNotFoundException => new ErrorDetails(
                StatusCodes.Status404NotFound,
                "Usuário não encontrado",
                exception.Message),

            TransferNotAuthorizedException => new ErrorDetails(
                StatusCodes.Status403Forbidden,
                "Transferência não autorizada",
                exception.Message),

            DomainException => new ErrorDetails(
                StatusCodes.Status422UnprocessableEntity,
                "Regra de negócio inválida",
                exception.Message),

            DbUpdateConcurrencyException => new ErrorDetails(
                StatusCodes.Status409Conflict,
                "Conflito de concorrência",
                "A carteira foi alterada por outra operação."),

            HttpRequestException => new ErrorDetails(
                StatusCodes.Status503ServiceUnavailable,
                "Serviço externo indisponível",
                "Não foi possível consultar o serviço externo."),

            TaskCanceledException
                when (!httpContext.RequestAborted.IsCancellationRequested)
                => new ErrorDetails(
                    StatusCodes.Status503ServiceUnavailable,
                    "Serviço externo indisponível",
                    "O serviço externo excedeu o tempo limite."),

            _ => new ErrorDetails(
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro inesperado.")
        };
    }

    private sealed record ErrorDetails(
        int StatusCode,
        string Title,
        string Detail);
}