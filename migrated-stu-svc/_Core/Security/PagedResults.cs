using System;

namespace ct._Core.Security
{
    public interface IPagedResults
    {
        bool Required { get; set; }
        //Search
        string SearchText { get; set; }
        //Sort
        string OrderBy { get; set; }
        string SortOrder { get; set; }
        bool OrderByDescending { get; }
        //Paging
        int Skip { get; set; }
        int Take { get; set; }
        //Results
        int CurrentPage { get; set; }
        int RecordsPerPage { get; set; }
        int TotalRecords { get; set; }
        int TotalPages { get; set; }
    }

    public class PagedResults : IPagedResults
    {
        public PagedResults()
        {
            Required = true;
            SortOrder = "asc";
        }

        public bool Required { get; set; }
        public string SearchText { get; set; }

        public string OrderBy { get; set; }
        public string SortOrder { get; set; }

        public bool OrderByDescending
        {
            get { return SortOrder.Equals("desc", StringComparison.InvariantCultureIgnoreCase); }
        }

        public int Skip { get; set; }
        public int Take { get; set; }

        public int CurrentPage { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}