using ct._Core.Service;
using System.Collections.Generic;
using System.Dynamic;

namespace ct._Core.Repo.Dto
{
    public class ReportDto
    {
        public object RH { get; set; }
        public List<Page> Pages { get; set; }
        public dynamic RF { get; set; }
        public PrintOptions Options { get; set; }
        public bool SinglePageReport { get; set; }
        public ReportDto()
        {
            Pages = new List<Page>();
        }
        public ReportDto(PrintOptions options = null) : this()
        {
            Options = options == null ? new PrintOptions() : options;
        }
        /// <summary>
        /// Report dto may contain dynamic properties. So serialize & deserialize to object
        /// </summary>
        /// <returns></returns>
        public object GetObject()
        {
            var json = JsonHelper.Serialize(this);
            var obj = JsonHelper.Deserialize(json);
            return obj;
        }
    }
    public class Page
    {
        public dynamic PH { get; set; }
        public PageTitle PT { get; set; }
        public dynamic PC { get; set; }
        public dynamic PF { get; set; }
        /// <summary>
        /// Show Page Border
        /// </summary>
        public bool PB { get; set; }
        /// <summary>
        /// Indicates if the page is a summary & not the regualr page representing data
        /// </summary>
        public bool IsSummary { get; set; }
        public Page(bool showPageBorder = false)
        {
            PT = new PageTitle();
            PB = showPageBorder;
        }
        public void SetHeader(ClientDto inst, int page_number, IPageOptions options)
        {
            PH = new ExpandoObject();
            var print_header = (page_number == 0 && options.PrintPH) || (page_number > 0 && options.PrintPH && options.PrintPHOnEachPage);
            if (print_header)
            {
                PH.Institute = inst.Name;
                PH.Logo = inst.Logo;
                PH.AddressLine1 = inst.Line1;
                PH.AddressLine2 = inst.Line2;
                PH.City = inst.City;
                if (!string.IsNullOrEmpty(inst.Line1) && !string.IsNullOrEmpty(inst.Pincode))
                {
                    PH.City += $" - {inst.Pincode}";
                }
            }
        }
        public void SetFooter(ClientDto inst, int page_number, IPageOptions options)
        {
            PF = null;
            var print_footer = (page_number == 0 && options.PrintPF) || (page_number > 0 && options.PrintPF && options.PrintPFOnEachPage);
        }

        public void SetTitle(string title1 = null, string title2 = null, string title3 = null, string title4 = null, int page_number = 0, IPageOptions options = null)
        {
            var print_title = (page_number == 0 && options.PrintPH) || (page_number > 0 && options.PrintPH && options.PrintPHOnEachPage);
            if (print_title) SetTitle(title1, title2, title3, title4);
        }

        public void SetTitle(string title1 = null, string title2 = null, string title3 = null, string title4 = null)
        {
            PT.Title1 = title1;
            PT.Title2 = title2;
            PT.Title3 = title3;
            PT.Title4 = title4;
        }
    }
    public class PageTitle
    {
        public string Title1 { get; set; }
        public string Title2 { get; set; }
        public string Title3 { get; set; }
        public string Title4 { get; set; }
    }
}