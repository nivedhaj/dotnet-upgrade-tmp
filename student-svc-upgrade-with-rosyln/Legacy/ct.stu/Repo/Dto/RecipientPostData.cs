using System.Collections.Generic;

namespace Repo.Dto
{
    /// <summary>
    /// Recipient Post Data
    /// </summary>
    public class RecipientPostData
    {
        /// <summary>
        /// Recipients
        /// </summary>
        public RecipientsFacet Recipients { get; set; }
        /// <summary>
        /// Text to be sent to recipients
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// SMS Reference Id
        /// </summary>
        /// <value></value>
        public string SMSRefId { get; set; }
        /// <summary>
        /// Tags
        /// </summary>
        /// <value></value>
        public bool IncludeTags { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        public RecipientPostData()
        {
            Recipients = new RecipientsFacet();
        }
    }
    /// <summary>
    /// Recipients Facet
    /// </summary>
    public class RecipientsFacet
    {
        /// Parent Facet C|T|S|Stu
        public List<List<string>> Parent { get; set; }
        /// <summary>
        /// Student Facet C|T|S|Stu
        /// </summary>
        public List<List<string>> Student { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        public RecipientsFacet()
        {
            Parent = new List<List<string>>();
            Student = new List<List<string>>();
        }
    }
}
