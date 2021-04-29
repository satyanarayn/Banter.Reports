using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Banter.Reports
{
    public class ErrorHandlingMiddleware
    {
        /// <summary>
        /// Defines the next
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        //private readonly ILogger<ErrorHandlingMiddleware> _logger;

        private readonly ILogger _logger;

        // private static IHttpContextAccessor _context;
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next<see cref="RequestDelegate"/></param>
        /// <param name="logger">The logger<see cref="ILogger{ErrorHandlingMiddleware}"/></param>
        public ErrorHandlingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            _logger = loggerFactory
                   .CreateLogger<ErrorHandlingMiddleware>();
        }

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="context">The context<see cref="HttpContext"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                // Call the next delegate/middleware in the pipeline
                await next(context);

                int code = context.Response.StatusCode;
                string errorMessage = string.Empty;
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    errorMessage = "Unauthorized";
                    await HandleExceptionAsync(context, errorMessage, code);
                }
                else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    errorMessage = "Forbidden";
                    await HandleExceptionAsync(context, errorMessage, code);
                }
                else if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    errorMessage = "Not Found";
                    await HandleExceptionAsync(context, errorMessage, code);
                }
                //else if (context.Response.StatusCode == StatusCodes.Status405MethodNotAllowed)
                //{                    
                //    errorMessage = "Method Not Allowed";
                //    await HandleExceptionAsync(context, errorMessage, code);
                //}
                else if (context.Response.StatusCode == StatusCodes.Status408RequestTimeout)
                {
                    errorMessage = "Request Timeout";
                    await HandleExceptionAsync(context, errorMessage, code);
                }
                else if (context.Response.StatusCode == StatusCodes.Status502BadGateway)
                {
                    errorMessage = "The server encountered a temporary error and could  not complete you request. Please wait moment and try again";
                    await HandleExceptionAsync(context, errorMessage, code);
                }
                else if (context.Response.StatusCode == StatusCodes.Status503ServiceUnavailable)
                {
                    errorMessage = "Service Unavailable";
                    await HandleExceptionAsync(context, errorMessage, code);
                }
                else if (context.Response.StatusCode == StatusCodes.Status504GatewayTimeout)
                {
                    errorMessage = "Looks like the server is taking to long to respond, this can be caused by either poor connectivity or an error with our servers. Please wait moment and try again";
                    await HandleExceptionAsync(context, errorMessage, code);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// The HandleExceptionAsync
        /// </summary>
        /// <param name="context">The context<see cref="HttpContext"/></param>
        /// <param name="exception">The exception<see cref="Exception"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            ScocuBaseModel basemodel = new ScocuBaseModel();
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            //if (exception is ScocuException)
            //{
            //    basemodel = JsonConvert.DeserializeObject<ScocuBaseModel>(exception.Message, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            //    code = ((ScocuException)exception).StatusCode;
            //}
            //else if (exception is DbUpdateException dbUpdateEx)
            //{
            //    if (dbUpdateEx.InnerException != null && dbUpdateEx.InnerException is SqlException sqlException)
            //    {
            //        if (sqlException.Number == 2601 || sqlException.Number == 2627)
            //        {
            //            code = HttpStatusCode.Conflict;
            //            basemodel = new ScocuBaseModel { Status = "error", Errors = new List<Error> { new Error { Code = "409", Message = "Cannot insert duplicate values." } } };
            //        }
            //        else
            //        {
            //            code = HttpStatusCode.InternalServerError;
            //            basemodel = new ScocuBaseModel { Status = "error", Errors = new List<Error> { new Error { Code = "500", Message = sqlException.Message } } };
            //        }
            //    }
            //    else
            //    {
            //        code = HttpStatusCode.InternalServerError;
            //        basemodel = new ScocuBaseModel { Status = "error", Errors = new List<Error> { new Error { Code = "500", Message = dbUpdateEx.Message } } };
            //    }
            //}
            //else
            //{
            //    code = HttpStatusCode.InternalServerError; // 500 if unexpected
            //    basemodel = new ScocuBaseModel { Status = "error", Errors = new List<Error> { new Error { Code = "500", Message = exception.Message } } };

            //}
            code = HttpStatusCode.InternalServerError; // 500 if unexpected
            basemodel = new ScocuBaseModel { Status = "error", Errors = new List<Error> { new Error { Code = "500", Message = exception.Message } } };
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(JsonConvert.SerializeObject(basemodel, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
        }

        /// <summary>
        /// The HandleExceptionAsync
        /// </summary>
        /// <param name="context">The context<see cref="HttpContext"/></param>
        /// <param name="exception">The exception<see cref="string"/></param>
        /// <param name="code">The code<see cref="int"/></param>
        /// <returns>The <see cref="Task"/></returns>
        private Task HandleExceptionAsync(HttpContext context, string exception, int code)
        {
            ScocuBaseModel basemodel = new ScocuBaseModel();
            basemodel = new ScocuBaseModel { Status = "error", Errors = new List<Error> { new Error { Code = code.ToString(), Message = exception } } };
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = code;
            string error = JsonConvert.SerializeObject(basemodel, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            _logger.LogError(error);
            return context.Response.WriteAsync(error);
        }
    }


    public class ScocuBaseModel
    {
        public ScocuBaseModel()
        {
            Errors = new List<Error>();
        }
        [JsonProperty("status", Order = 1)]
        public string Status { get; set; }
        public List<Error> Errors { get; set; }

        public bool ShouldSerializeErrors()
        {
            if (Errors.Count == 0)
                return false;
            else
                return true;
        }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
