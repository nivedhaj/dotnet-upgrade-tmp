using ct._Core.Repo.Resource;
using Microsoft.Extensions.Configuration;
using Marten;
using Marten.Linq;
using System;

namespace ct._Core.Repo.Db.Command
{
    public class PingCmd
    {
        private readonly IDocumentSession _session;
        private readonly IConfiguration _config;

        public PingCmd(IDocumentSession session, IConfiguration config)
        {
            _session = session;
            _config = config;
        }    

        //Create:: string -> string -> Ping a
        public Ping Create(string message)
        {
            string serviceName = _config.GetSection("Service:Name").Value;
            var ping = new Ping() { Service = serviceName, Message = message };
            if (ping.IsValid())
            {
                _session.Store(ping);
                _session.SaveChanges();
            }
            else
            {
                throw new ArgumentException(nameof(ping));
            }
            return ping;
        }

    }
}