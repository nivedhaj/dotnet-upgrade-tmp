using Microsoft.AspNetCore.Mvc.Formatters;

namespace ct._Core.Util
{
    /// <summary>
    /// Html Output Formatter to support HTML => text/html
    /// </summary>
    public class HtmlOutputFormatter : StringOutputFormatter
    {
        /// <summary>
        /// Ctor for HtmlOutputFormatter
        /// </summary>
        public HtmlOutputFormatter()
        {
            SupportedMediaTypes.Add("text/html");
        }
    }
}
