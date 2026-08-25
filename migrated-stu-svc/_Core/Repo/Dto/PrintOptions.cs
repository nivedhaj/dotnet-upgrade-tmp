using System;

namespace ct._Core.Repo.Dto
{
    /// <summary>
    /// Print Options
    /// </summary>
    public class PrintOptions : IPageOptions
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public PrintOptions()
        {
            PrintRH = true;
            PrintRF = true;
            PrintPH = true;
            PrintPHOnEachPage = true;
            PrintRF = true;
            PrintOn = true;
            PrintBy = true;
            On = string.Format("{0:dd MMM yyyy hh:mm}", DateTime.Now);
        }
        /// <summary>
        /// Records Per Page
        /// </summary>
        public int RecordsPerPage { get; set; } //Chunk size
        /// <summary>
        /// Print Report Header
        /// </summary>
        public bool PrintRH { get; set; }
        /// <summary>
        /// Print Report Footer
        /// </summary>
        public bool PrintRF { get; set; }
        /// <summary>
        /// Print Page Header
        /// </summary>
        public bool PrintPH { get; set; }
        /// <summary>
        /// Print Header On Each PAge
        /// </summary>
        public bool PrintPHOnEachPage { get; set; }
        /// <summary>
        /// Print Page Footer
        /// </summary>
        public bool PrintPF { get; set; }
        /// <summary>
        /// Print Page Footer On Each Page
        /// </summary>
        public bool PrintPFOnEachPage { get; set; }
        /// <summary>
        /// Print On
        /// </summary>
        public bool PrintOn { get; set; }
        /// <summary>
        /// Print By
        /// </summary>
        public bool PrintBy { get; set; }
        /// <summary>
        /// On
        /// </summary>
        public string On { get; set; }
        /// <summary>
        /// By
        /// </summary>
        public string By { get; set; }
        /// <summary>
        /// Print Border
        /// </summary>
        public bool PrintBorder { get; set; }
    }
    /// <summary>
    /// Page Options Interface
    /// </summary>
    public interface IPageOptions
    {
        /// <summary>
        /// Print Page Header
        /// </summary>
        bool PrintPH { get; set; }
        /// <summary>
        /// Print Page Footer On Each Page
        /// </summary>
        bool PrintPHOnEachPage { get; set; }
        /// <summary>
        /// Print Page Footer
        /// </summary>
        bool PrintPF { get; set; }
        /// <summary>
        /// Print Page Footer On Each Page
        /// </summary>
        bool PrintPFOnEachPage { get; set; }
        /// <summary>
        /// Print On
        /// </summary>
        bool PrintOn { get; set; }
        /// <summary>
        /// Print By
        /// </summary>
        bool PrintBy { get; set; }
        /// <summary>
        /// Print Border
        /// </summary>
        bool PrintBorder { get; set; }
    }
}
