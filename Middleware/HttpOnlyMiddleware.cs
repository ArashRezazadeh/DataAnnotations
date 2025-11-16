using DataAnnotations.Errors;

namespace DataAnnotations.Middleware;

// Register the middleware and configure forwarded headers(in Program.cs). 
public class HttpOnlyMiddleware
{
    private readonly RequestDelegate _next;

    public HttpOnlyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.IsHttps)
        {
            var problemDetails = new CustomHttpProblemDetails(context);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);

            return;
        }

        await _next(context);
    }
}