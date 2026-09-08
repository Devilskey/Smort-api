using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace Smort_api.Handlers.Security;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler>  logger)
: IExceptionHandler
{

    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        
        logger.LogError(exception, $"Unhandled error with Traceid: {traceId}", traceId);

        var (error, message, detail) = exception switch
        {
            MySqlException => ("DatabaseError", "A database operation failed", "Check the database operation and connection"),
            TimeoutException => ("TimeOut", "The Operation timed out", "The requested operation took too long to complete"),
            HttpRequestException => ("ExternalService error", " an external service could not react", "Check your network connection and check if the service is down"),
            FileNotFoundException => ("FileNotFound", "The file could not be found", "Check the file path"),
            FieldAccessException => ("No Access", "Could not access the file", "Check the file path or try again later"),
            _ => ("InteralError", "An unexpected Error occurred", null)
        };

        httpContext.Response.WriteAsJsonAsync(new ApiError(
            500,
            error,
            message,
            detail,
            traceId), 
            cancellationToken
        );
            
        return ValueTask.FromResult(true);
    }
}

public sealed record ApiError(
    int Status, 
    string Error, 
    string Message, 
    string? Detail, 
    string TraceId);