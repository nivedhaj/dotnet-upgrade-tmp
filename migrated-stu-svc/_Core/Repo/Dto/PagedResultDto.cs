using System;
using System.Collections.Generic;

namespace ct._Core.Repo.Dto
{
    internal class PagedResultDto<T>
    {
        public List<T> Data { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages
        {
            get { return (int)Math.Ceiling((double)this.TotalRecords / this.RecordsPerPage); }
        }
    }
}
