using ct._Core.Repo.Resource;
using Raven.Client.Documents.Indexes;
using System.Linq;

namespace ct._Core.Repo.Db.Index
{
    public class Ping_Index : AbstractIndexCreationTask<Ping>
    {
        public Ping_Index()
        {
            Map =
                pings =>
                from ping in pings
                select new { ping.At, ping.Service };
        }
    }
}