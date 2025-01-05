using Microsoft.AspNetCore.Http;
using NLog;
using System.Diagnostics;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Logger RequestLogger = LogManager.GetLogger("request-logger");
    private static int requestCounter = 0;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        int currentRequestNumber = Interlocked.Increment(ref requestCounter);
        MappedDiagnosticsLogicalContext.Set("RequestNumber", currentRequestNumber);

        var stopwatch = Stopwatch.StartNew();

        RequestLogger.Info($"Incoming request | #{currentRequestNumber} | resource: {context.Request.Path} | HTTP Verb {context.Request.Method.ToUpper()}");

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            RequestLogger.Error(ex, $"Request #{currentRequestNumber} failed: {ex.Message}");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            RequestLogger.Debug($"Request #{currentRequestNumber} duration: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
