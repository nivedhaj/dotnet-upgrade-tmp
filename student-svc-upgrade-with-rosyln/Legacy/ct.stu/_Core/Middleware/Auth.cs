using ct._Core.Security;
using ct._Core.Service;
using ct._Core.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ct._Core.Middleware
{
    /// <summary>
    /// Currently doesn't do anything. Going forward will validate requests and hydrate shared DTOs?
    /// </summary>
    public class AuthMiddleware
    {
        readonly RequestDelegate _next;
        HttpContext _context;
        private readonly ILogger<AuthMiddleware> _logger;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        /// <summary>
        /// Called from Runtime
        /// </summary>
        /// <param name="context"></param>
        /// <param name="uc"></param>
        /// <param name="rabbitMQService"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, IUserContext uc, RabbitMQService rabbitMQService)
        {
            //Execution will reach here iff AccessCode is valid. Otherwise an error is thrown during construction of dependency IUserContext.

            _context = context;

            var end_point = $"{_context.Request.Method}{_context.Request.Path.Value}".Replace("/api", "").ToLower();
            end_point = end_point.TrimEnd('/');
            var token_urls = new List<string>() {
                "get/auth/token",
                "get/auth/token/etl",
                "get/auth/token/bootstrap",
                "post/auth/anon/resetpassword",
                "get/auth/anon/confirmregistration",
                "get/stu/_monitor/ping"
            };
            var check_token = !token_urls.Contains(end_point);
            var check_scope = !_context.Request.Path.Value.StartsWith("/api/_") && !_context.Request.Path.Value.StartsWith("/api/stu/_monitor") && !end_point.Contains("swagger");
            var audit_log = new AuditLog()
            {
                URL = UriHelper.GetDisplayUrl(context.Request),
                IP = context.Connection.RemoteIpAddress.ToString(),
                App = Helper.GetHeader<string>(context.Request, _Constants.ACCESS_CODE),
                UE = Helper.GetHeader<string>(context.Request, _Constants.USER_EMAIL)//gets filled for auth/token requests
            };
            context.Items[_Constants.AUDIT_LOG] = audit_log;
            var _topic = "";
            if (check_token && check_scope)
            {
                var _uc = uc.Current(); //Checks for Token and throws if in error
                _topic = _uc.Topic;
                audit_log.App = _uc.AppId;
                audit_log.SId = _uc.SessionId;
                audit_log.UE = _uc.UserEmail;
                if (!_uc.IsInScope(end_point))
                {
                    //audit log will be handled by exception handler
                    throw new NotInScopeException(nameof(NotInScopeException));
                }
            }

            await _next(context);

            audit_log.Status = context.Response.StatusCode;
            audit_log.Out = DateTime.UtcNow;
            if (!end_point.Contains("ping"))
            {
                var job = new Job() { title = _Constants.AUDIT_LOG, payload = audit_log, topic = _topic };
                try
                {
                    rabbitMQService.EnqueJob(job);
                }
                catch (Exception ex)
                {
                    _logger.LogError("RabbitMQ Error:" + ex.Message);
                }
            }
        }
    }
}