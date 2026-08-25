using System.Collections.Generic;
using System.Linq;
using Raven.Client.Documents.Indexes;
using Repo.Resource;

namespace Repo.Db.Index
{
    internal class Student_Essentials_IX : AbstractIndexCreationTask<StudentEssentials>
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