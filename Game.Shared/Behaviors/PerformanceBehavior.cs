using Serilog;
using System.Diagnostics;

namespace Game.Shared.Behaviors;

/// <summary>
/// Logs warnings for slow commands/queries (>500ms).
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const int SlowThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowThresholdMs)
        {
            var requestName = typeof(TRequest).Name;
            Log.Warning("Slow request detected: {RequestName} took {ElapsedMs}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
