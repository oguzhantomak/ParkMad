namespace BuildingBlocks.Exceptions.Handler;

public class CustomExceptionHandler(ILogger<CustomExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError("Error Message: {exceptionMessage}, Time of occurence {time}", exception.Message, DateTime.UtcNow);

        //Create anonymous object to store the details of the exception
        (string Detail, string Title, int StatusCode) details = exception switch
        {
            InternalServerException => (
                exception.Message,
                exception.GetType().Name,
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError
                ),
            BadRequestException => (
                exception.Message,
                exception.GetType().Name,
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest
                ),
            NotFoundException => (
                exception.Message,
                exception.GetType().Name,
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound
                ),
            ValidationException => (
                exception.Message,
                exception.GetType().Name,
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest
                ),
            _ => (
                exception.Message,
                exception.GetType().Name,
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError
                )
        };

        //Create a ProblemDetails object to store the details of the exception
        var problemDetails = new ProblemDetails
        {
            Title = details.Title,
            Status = details.StatusCode,
            Detail = details.Detail
        };

        problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

        return true;
    }
}