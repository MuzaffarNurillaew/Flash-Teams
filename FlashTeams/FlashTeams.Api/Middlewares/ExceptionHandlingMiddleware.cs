using FlashTeams.Shared.Exceptions;
using FluentValidation;

namespace FlashTeams.Api.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next.Invoke(httpContext);
        }
        catch (NotFoundBaseException exception)
        {
            logger.LogError(exception, exception.Message);
            await HandleExceptionAsync(httpContext, exception, NotFoundBaseException.Code);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(exception, exception.Message);
            await HandleExceptionAsync(httpContext, exception, 400);
        }
        catch (FlashTeamsException exception)
        {
            logger.LogError(exception, exception.Message);
            await HandleExceptionAsync(httpContext, exception, exception.Code);
        }
        catch (ValidationException exception)
        {
            logger.LogError(exception, exception.Message);
            await HandleValidationExceptionAsync(httpContext, exception, 400);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, exception.Message);
            await HandleExceptionAsync(httpContext, exception, 500);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext httpContext, ValidationException exception, int code)
    {
        httpContext.Response.StatusCode = code;

        await httpContext.Response.WriteAsJsonAsync(exception.Errors.Select(error =>
            new
            {
                error.PropertyName,
                error.ErrorMessage,
            }));
    }

    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception, int code)
    {
        httpContext.Response.StatusCode = code;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            Message = exception.Message,
        });
    }
}