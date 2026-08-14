// Migrated from RavenDB index → Postgres/Marten.
// The nested Result types are kept for query projections.
// Raven Map/Reduce definitions are no longer executed; use Marten LINQ
// against documents or configure property indexes in StudentModule CreateStore.
using ct._Core.Repo.Resource;
using Marten.Schema;
using System.Linq;

namespace ct._Core.Repo.Db.Index
{
    public class Ping_Index 
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