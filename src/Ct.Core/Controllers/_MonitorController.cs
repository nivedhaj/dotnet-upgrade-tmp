using ct._Core.Repo.Db.Command;
using ct._Core.Repo.Db.Query;
using ct._Core.Repo.Resource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Marten;
using Marten.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ct._Core.Controllers
{
    /// <summary>
    /// Monitors the health of services
    /// </summary>
    [Route("api/stu/[Controller]")]
    public class _MonitorController : Controller
    {
        private readonly ILogger<_MonitorController> _logger;
        private readonly IDocumentStore _store;
        private readonly PingCmd _pingCmd;
        private readonly PingQry _pingQry;

        public _MonitorController(ILogger<_MonitorController> logger, IDocumentStore store, PingCmd pingCmd, PingQry pingQry)
        {
            _logger = logger;
            _store = store;
            _pingCmd = pingCmd;
            _pingQry = pingQry;
        }

        [HttpGet("ping")]
        public IActionResult ping()
        {
            return Json("pong");
        }

        // Post:: string -> HTTP_Status 201
        [HttpPost("db/index")]
        public void CreateIndexes()
        {
            // deploy all indexes 
            var asm = typeof(Ping).Assembly;
            IndexCreation.CreateIndexes(asm, _store);
        }

        [HttpGet("db/ping")]
        public IEnumerable<Ping> Get()
        {
            return _pingQry.Get();
        }

        // Post:: string -> HTTP_Status 201
        [HttpPost("db/ping")]
        public void Post([FromQuery]string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message));
            }

            _pingCmd.Create(message);
            Response.StatusCode = 201;
        }

        [HttpGet("endpoints")]
        public IActionResult GetEndPoints()
        {
            var op_ids = new List<string>();
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri($"http://{HttpContext.Request.Host.Value}");
                var json = client.GetStringAsync("swagger/v1/swagger.json").Result;
                var jobj = JObject.Parse(json);
                var op_id_tokens = jobj.SelectTokens("$..operationId").Select(t => t.ToObject<string>()).ToList();
                foreach (var s in op_id_tokens)
                {
                    try
                    {
                        var token = Regex.Replace(s, "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}");
                        var arr = token.Split(null);
                        if (arr[1].Equals("admin", StringComparison.CurrentCultureIgnoreCase)) continue;
                        var _arr = new List<string> { arr.Last() }.Concat(arr.Skip(1).Reverse().Skip(1).Reverse());
                        op_ids.Add(string.Join("/", _arr).ToLower());
                    }
                    catch { }
                }
            }
            return Json(op_ids);
        }

        [HttpPost("log/error")]
        public void LogError([FromQuery]string msg)
        {
            _logger.LogError($"{msg}");
        }

        [HttpPost("log/info")]
        public void LogInfo([FromQuery]string msg)
        {
            _logger.LogInformation($"{msg}");
        }

        [HttpPost("log/warning")]
        public void LogWarning([FromQuery]string msg)
        {
            _logger.LogWarning($"{msg}");
        }

        
    }
}
