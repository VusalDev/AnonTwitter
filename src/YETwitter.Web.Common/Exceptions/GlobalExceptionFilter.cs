using Hellang.Middleware.ProblemDetails;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace YETwitter.Web.Common
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment hostEnvironment;
        private readonly ILogger logger;

        public GlobalExceptionFilter(IHostEnvironment hostEnvironment, ILogger<GlobalExceptionFilter> logger)
        {
            this.hostEnvironment = hostEnvironment;
            this.logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            this.logger.LogError(context.Exception, context.Exception?.StackTrace);

            //if (hostEnvironment.IsDevelopment())
            //{
            //    // Don't display exception details unless running in Development.
            //    System.Diagnostics.Debugger.Break();
            //}

            switch (context.Exception)
            {
                case ProblemDetailsException:
                    return;
                case ArgumentException or ArgumentNullException:
                    var problemDetails = new ProblemDetails { Status = 400, Title = "Bad Request", Detail = context.Exception.Message };
                    context.HttpContext.Response.StatusCode = 400;
                    context.Exception = new ProblemDetailsException(problemDetails);
                    return;
                case InvalidOperationException:
                    problemDetails = new ProblemDetails() { Status = 422, Title = "Unprocessable Entity", Detail = context.Exception.Message };
                    context.HttpContext.Response.StatusCode = 422;
                    context.Exception = new ProblemDetailsException(problemDetails);
                    break;
            }
            return;
        }
    }
}
