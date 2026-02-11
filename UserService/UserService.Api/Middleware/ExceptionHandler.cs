using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Exceptions;

namespace UserService.Api.Middleware;

public class ExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = ex switch
            {
                ApplicationException => StatusCodes.Status400BadRequest,
                InvalidCredentialException => StatusCodes.Status401Unauthorized,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                DbCreatingException => StatusCodes.Status422UnprocessableEntity,
                FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError,
            };
            
            _logger.LogError(ex, "Unhandled error occured");

            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Type = ex.GetType().Name,
                    Status = context.Response.StatusCode,
                    Detail = ex.Message,
                    Instance = context.Request.Path,
                });

        }
    }
}