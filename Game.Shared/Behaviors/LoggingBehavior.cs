using Serilog;
using System.Diagnostics;

namespace Game.Shared.Behaviors;

/// <summary>
/// Logs all commands and queries with timing information.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        Log.Information("Executing {RequestName}", requestName);
        Log.Debug("Request: {@Request}", request);

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            Log.Information("Completed {RequestName} in {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(ex, "Failed {RequestName} after {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
