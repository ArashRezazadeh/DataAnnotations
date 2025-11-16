using Microsoft.AspNetCore.Mvc;

namespace DataAnnotations.Errors;

public class CustomHttpProblemDetails : ValidationProblemDetails
{
    public CustomHttpProblemDetails(HttpContext context)
    {
        Title = "Bad Request";
        Status = StatusCodes.Status400BadRequest;
        Detail = "HTTP requests are not allowed. Please use HTTPS.";
        Instance = $"{context.Request.Path} ({context.TraceIdentifier})";

        Dictionary<string, string?> relevantHeaders = new Dictionary<string, string?>
        {
            { "Host", context.Request.Headers["Host"] },
            { "User-Agent", context.Request.Headers["User-Agent"] },
            { "X-Forwarded-Proto", context.Request.Headers["X-Forwarded-Proto"] },
            { "X-Forwarded-For", context.Request.Headers["X-Forwarded-For"] }
        };

        Extensions["headers"] = relevantHeaders;
    }
}

// Why ValidationProblemDetails?
// ValidationProblemDetails is the default response type for HTTP 400 responses. 
// This class is more often used with InvalidModelStateResponseFactory to 
// convert invalid ModelStateDIctionary objects into IActionResult. However, 
// ValidationProblemDetails is also used for other types of validation errors, including 
// rejecting HTTP requests. We are simply inheriting from ValidationProblemDetails 
// to maintain consistency with the default response type for HTTP 400 responses.