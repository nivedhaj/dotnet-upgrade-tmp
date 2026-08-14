// Migrated from RavenDB index → Postgres/Marten.
// The nested Result types are kept for query projections.
// Raven Map/Reduce definitions are no longer executed; use Marten LINQ
// against documents or configure property indexes in StudentModule CreateStore.
using System.Collections.Generic;
using System.Linq;
using Marten.Schema;
using Repo.Resource;

namespace Repo.Db.Index
{
    internal class Student_Essentials_IX 
    {
        public class Result
        {
            public string Id { get; set; }
            public string StudentId { get; set; }
        }

        public Student_Essentials_IX()
        {
            Map = itemRecords => from record in itemRecords
                                  select new Result
                                  {
                                      Id = record.Id,
                                      StudentId = record.StudentId
                                  };
            StoreAllFields(FieldStorage.Yes);
        }
    }
}