using System.Net;
using System.Text.Json;
using StoreApp.Application.Exceptions;

namespace StoreApp.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var message = exception.Message;

            switch (exception)
            {
                case NotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                    break;
                case ConflictException:
                    response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                    message = "An internal server error occurred.";
                    break;
            }

            var errorResponse = new
            {
                success = false,
                message = message
            };

            var result = JsonSerializer.Serialize(errorResponse);
            return response.WriteAsync(result);
        }
    }
}