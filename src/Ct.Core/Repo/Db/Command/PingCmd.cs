using ct._Core.Repo.Resource;
using Microsoft.Extensions.Configuration;
using ct._Core.Data;
using System;

namespace ct._Core.Repo.Db.Command
{
    public class PingCmd
    {
        private readonly IPostgresSession _session;
        private readonly IConfiguration _config;

        public PingCmd(IPostgresSession session, IConfiguration config)
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