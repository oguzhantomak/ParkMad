namespace BuildingBlocks.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Log the request
        logger.LogInformation("[START] Handle request ={Request} - Response={Response} - RequestData={RequestData}", typeof(TRequest).Name, typeof(TResponse).Name, request);

        var timer = new Stopwatch();
        timer.Start();

        var response = await next();

        timer.Stop();
        var timeTaken = timer.Elapsed;

        // Log performance if the request takes more than 3 seconds
        if (timeTaken.Seconds > 3)
            logger.LogWarning("[PERFORMANCE] The request={Request} took {TimeTaken} seconds.", typeof(TRequest).Name, timeTaken.Seconds);

        // Log the request and response
        logger.LogInformation("[END] Handle request ={Request} - Response={Response} - ElapsedTime={ElapsedTime} - RequestData={RequestData}", typeof(TRequest).Name, typeof(TResponse).Name, timer.Elapsed, request);

        return response;
    }
}