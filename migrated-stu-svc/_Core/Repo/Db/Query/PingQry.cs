using ct._Core.Repo.Db.Index;
using ct._Core.Repo.Resource;
using Microsoft.Extensions.Configuration;
using ct._Core.Persistence;
using System.Collections.Generic;
using System.Linq;


namespace ct._Core.Repo.Db.Query
{
    public class PingQry
    {
        private readonly IConfiguration _config;
        private readonly IPostgresSession _session;

        public PingQry(IPostgresSession session, IConfiguration config)
        {
            _session = session;
            _config = config;
        }
        public IEnumerable<Ping> Get()
        {
            string serviceName = _config.GetSection("Service:Name").Value;
            var qry = _session.Query<Ping, Ping_Index>();
            var result = qry
                    .Where(o => o.Service == serviceName)
                    .OrderByDescending(o => o.At)
                    .OfType<Ping>();
            return result;
        }
    }
}