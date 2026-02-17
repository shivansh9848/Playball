using System.Net;
using System.Text.Json;
using Assignment_Example_HU.Application.DTOs.Response;
using Assignment_Example_HU.Common.Exceptions;

namespace Assignment_Example_HU.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred";
        IDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                message = validationEx.Message;
                errors = validationEx.Errors;
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                break;

            case UnauthorizedException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                message = unauthorizedEx.Message;
                break;

            case BusinessException businessEx:
                statusCode = HttpStatusCode.BadRequest;
                message = businessEx.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.ErrorResponse(message, errors);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
