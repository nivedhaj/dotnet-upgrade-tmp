using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ct.stu.Service
{
    //ref: http://stackoverflow.com/questions/769621/dealing-with-commas-in-a-csv-file
    /// <summary>
    /// Provides Csv helper methods
    /// </summary>
    public class CsvService
    {
        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static readonly char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };
        private static readonly Regex rexCsvSplitter = new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))");
        private static readonly Regex rexRunOnLine = new Regex(@"^[^""]*(?:""[^""]*""[^""]*)*""[^""]*$");
        private readonly TextReader __reader;

        /// <summary>
        /// Builds CSV from a list
        /// </summary>
        /// <param name="input"></param>
        /// <param name="extraColumns"></param>
        public string GetCSV<T>(List<T> input, List<string> extraColumns = null)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder();
            var delimiter = ",";

            //header
            foreach (var prp in properties)
            {
                if (prp.CanRead)
                    sb.Append(prp.Name).Append(delimiter);
            }
            if (extraColumns != null && extraColumns.Count > 0)
            {
                foreach (var col in extraColumns)
                {
                    sb.Append(col).Append(delimiter);
                }
            }
            sb.Length--; // Remove last delimiter
            sb.AppendLine();

            //content
            foreach (var t in input)
            {
                foreach (var prp in properties)
                {
                    if (prp.CanRead)
                    {
                        sb.Append(prp.GetValue(t, null)).Append(delimiter);
                    }
                }
                if (extraColumns != null && extraColumns.Count > 0)
                {
                    foreach (var col in extraColumns)
                    {
                        sb.Append(delimiter); //Just add blank values
                    }
                }
                sb.Length--; // Remove last delimiter
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the ByteArray Content
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static ByteArrayContent GetByteArrayContent(string csv)
        {
            var stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(csv);
            writer.Flush();
            stream.Position = 0;
            var result = new ByteArrayContent(stream.ToArray());
            return result;
        }

        /// <summary>
        /// Escapes characters
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Escape(string s)
        {
            if (s.Contains(QUOTE))
                s = s.Replace(QUOTE, ESCAPED_QUOTE);

            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                s = QUOTE + s + QUOTE;

            return s;
        }

        /// <summary>
        /// Unescape
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Unescape(string s)
        {
            if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(ESCAPED_QUOTE))
                    s = s.Replace(ESCAPED_QUOTE, QUOTE);
            }

            return s;
        }

        /// <summary>
        /// Row Index
        /// </summary>
        /// <returns></returns>
        public long RowIndex { get; private set; }

        /// <summary>
        /// Dispose Reader
        /// </summary>
        public void Dispose()
        {
            if (null != __reader) __reader.Dispose();
        }
    }

}