using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using JWT.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ct._Core.Middleware
{
    /// <summary>
    /// All unhandled exceptions are managed by this middleware
    /// </summary>
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        public ExceptionHandlerMiddleware(RequestDelegate next) => this.next = next;

        /// <summary>
        /// Called by FW
        /// </summary>
        /// <param name="context"></param>
        /// <param name="config"></param>
        /// <param name="rabbitMQService"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IConfiguration config, RabbitMQService rabbitMQService)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await ExceptionHandlerMiddleware.handleExceptionAsync(context, ex, config, rabbitMQService);
            }
        }
        private static Task handleExceptionAsync(HttpContext context, Exception exception, IConfiguration config, RabbitMQService rabbitMQService)
        {
            bool handle_errors = config["Ct:Handle.Errors"] == "true";
            var audit_log = (AuditLog)context.Items[_Constants.AUDIT_LOG];
            if (handle_errors)
            {
                var httpStatus = HttpStatusCode.InternalServerError; // 500 if unexpected
                var base_exception = exception.GetBaseException();
                var result = JsonConvert.SerializeObject(new { error = base_exception.Message });

                if (base_exception is ArgumentException) httpStatus = HttpStatusCode.BadRequest;
                else if (base_exception is ArgumentNullException) httpStatus = HttpStatusCode.BadRequest;
                else if (base_exception is ArgumentOutOfRangeException) httpStatus = HttpStatusCode.BadRequest;
                else if (base_exception is DuplicateObjectException) { httpStatus = HttpStatusCode.BadRequest; result = JsonConvert.SerializeObject(new { error = $"{nameof(DuplicateObjectException)}:{base_exception.Message}" }); }
                else if (base_exception is InvalidAccessCodeException) httpStatus = HttpStatusCode.BadRequest;
                else if (base_exception is InvalidAccessTokenException) httpStatus = HttpStatusCode.BadRequest;
                else if (base_exception is InvalidCredentialsException) httpStatus = HttpStatusCode.BadRequest;
                else if (base_exception is NotImplementedException) httpStatus = HttpStatusCode.NotImplemented;
                else if (base_exception is NotInScopeException) httpStatus = HttpStatusCode.Forbidden;
                else if (base_exception is ObjectNotFoundException) { httpStatus = HttpStatusCode.BadRequest; result = JsonConvert.SerializeObject(new { error = $"{nameof(ObjectNotFoundException)}:{base_exception.Message}" }); }
                else if (base_exception is TokenExpiredException) { httpStatus = HttpStatusCode.Gone; result = JsonConvert.SerializeObject(new { error = nameof(TokenExpiredException) }); }
                else if (base_exception is UserEmailAlreadyRegisteredException) httpStatus = HttpStatusCode.Conflict;
                else  //unhandled exception
                {
                    var messages = exception.FromHierarchy(ex => ex.InnerException).Select(ex => ex.Message);
                    var msg = string.Join(Environment.NewLine, messages);
                    result = JsonConvert.SerializeObject(new { error = msg, error_detail = exception.ToString() });

                    Console.WriteLine(exception.ToString());

                    audit_log.Status = (int)httpStatus;
                    audit_log.Message = msg;
                    audit_log.Out = DateTime.UtcNow;
                    var routing_suffix = _Constants.AUDIT_LOG;
                    var job = new Job() { title = _Constants.AUDIT_LOG, payload = audit_log };
                    rabbitMQService.EnqueJob(job);
                }
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)httpStatus;
                return context.Response.WriteAsync(result);
            }
            else // treat all as unhandled
            {
                var httpStatus = HttpStatusCode.InternalServerError;
                var messages = exception.FromHierarchy(ex => ex.InnerException).Select(ex => ex.Message);
                var msg = string.Join(Environment.NewLine, messages);
                var result = JsonConvert.SerializeObject(new { error = msg, error_detail = exception.ToString() });
                Console.WriteLine(exception.ToString());

                audit_log.Status = (int)httpStatus;
                audit_log.Message = msg;
                audit_log.Out = DateTime.UtcNow;
                var routing_suffix = _Constants.AUDIT_LOG;
                var job = new Job() { title = _Constants.AUDIT_LOG, payload = audit_log };
                rabbitMQService.EnqueJob(job);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)httpStatus;
                return context.Response.WriteAsync(result);
            }
        }
    }
}