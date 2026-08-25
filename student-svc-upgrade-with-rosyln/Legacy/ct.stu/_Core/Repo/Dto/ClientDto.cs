using System;

namespace ct._Core.Repo.Dto
{
    /// <summary>
    /// Client Dto
    /// </summary>
    public class ClientDto
    {
        /// <summary>
        /// Id of Org / Inst
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Name of Org / Inst
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Url of the client logo
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// Line 1
        /// </summary>
        /// <returns></returns>
        public string Line1 { get; set; }
        /// <summary>
        /// Line 2
        /// </summary>
        /// <returns></returns>
        public string Line2 { get; set; }
        /// <summary>
        /// City
        /// </summary>
        /// <returns></returns>
        public string City { get; set; }
        /// <summary>
        /// State
        /// </summary>
        /// <returns></returns>
        public string State { get; set; }
        /// <summary>
        /// Country
        /// </summary>
        /// <returns></returns>
        public string Country { get; set; }
        /// <summary>
        /// Pin code
        /// </summary>
        /// <returns></returns>
        public string Pincode { get; set; }
        /// <summary>
        /// Academic Year From
        /// </summary>
        public DateTime? AcademicYearFrom { get; set; }
        /// <summary>
        /// Academic Year To
        /// </summary>
        public DateTime? AcademicYearTo { get; set; }
        /// <summary>
        /// Org if True else Inst
        /// </summary>
        public bool IsOrg { get; set; }
        /// <summary>
        /// Short name
        /// </summary>
        /// <returns></returns>
        public string ShortName { get; set; }
        /// <summary>
        /// SMS Sender id
        /// </summary>
        /// <returns></returns>
        public string SMSSenderId { get; set; }
    }
}