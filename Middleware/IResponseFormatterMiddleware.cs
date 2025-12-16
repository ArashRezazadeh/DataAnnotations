namespace DataAnnotations.Middleware;



public interface IResponseFormatterMiddleware : IMiddleware
{
    string GetContentType();
}