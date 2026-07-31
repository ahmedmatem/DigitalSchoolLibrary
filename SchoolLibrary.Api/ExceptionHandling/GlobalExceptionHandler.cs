using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolLibrary.Application.Common.Exceptions;

namespace SchoolLibrary.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            ProblemDetails problemDetails;

            switch (exception)
            {
                case ValidationException validationException:
                    problemDetails = CreateValidationProblem(
                        httpContext,
                        validationException);
                    break;

                case NotFoundException:
                    problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Обектът не е намерен.",
                        Detail = exception.Message,
                        Instance = httpContext.Request.Path
                    };
                    break;

                default:
                    problemDetails = new ProblemDetails
                    {
                        Status =
                            StatusCodes.Status500InternalServerError,
                        Title = "Възникна вътрешна грешка.",
                        Detail =
                            "Заявката не можа да бъде обработена.",
                        Instance = httpContext.Request.Path
                    };
                    break;
            }

            var statusCode = problemDetails.Status
                ?? StatusCodes.Status500InternalServerError;

            if (statusCode >= 500)
            {
                logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);
            }
            else
            {
                logger.LogWarning(
                    "Request failed with {StatusCode}: {Message}",
                    statusCode,
                    exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }

        private static ValidationProblemDetails CreateValidationProblem(
            HttpContext httpContext,
            ValidationException exception)
        {
            var errors = exception.Errors.Count > 0
                ? exception.Errors
                : new Dictionary<string, string[]>
                {
                    ["general"] = [exception.Message]
                };

            return new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Грешка при валидацията.",
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };
        }
    }
}
