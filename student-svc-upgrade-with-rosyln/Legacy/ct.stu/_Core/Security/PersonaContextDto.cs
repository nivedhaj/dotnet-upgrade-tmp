using System;
using System.Collections.Generic;

namespace ct._Core.Security
{
    /// <summary>
    /// Persona Context Dto
    /// </summary>
    public class PersonaContextDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Persona Type
        /// </summary>
        public string PersonaType { get; set; } //PersonaTypeEnum.ToString()
        /// <summary>
        /// Client Id
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// Client Name
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// Client Short Name
        /// </summary>
        public string ClientShortName { get; set; }
        /// <summary>
        /// Client Logo Url
        /// </summary>
        public string ClientLogoUrl { get; set; }
        /// <summary>
        /// SMS Sender If
        /// </summary>
        public string SMSSenderId { get; set; }
        /// <summary>
        /// Is Org?
        /// </summary>
        public bool IsOrg { get; set; }
        /// <summary>
        /// Is Root ?
        /// </summary>
        public bool IsRoot { get; set; }
        /// <summary>
        /// Is Inst Locked?
        /// </summary>
        public bool IsInstituteLocked { get; set; }
        /// <summary>
        /// Parental Access Enabled ?
        /// </summary>
        public bool? ParentalAccessEnabled { get; set; }
        /// <summary>
        /// Staff Access Enabled ?
        /// </summary>
        public bool? StaffAccessEnabled { get; set; }
        /// <summary>
        /// Student Access Enabled?
        /// </summary>
        public bool? StudentAccessEnabled { get; set; }
        /// <summary>
        /// Acad From
        /// </summary>
        public DateTime? AcadYearFrom { get; set; }
        /// <summary>
        /// Acad To
        /// </summary>
        public DateTime? AcadYearTo { get; set; }
        /// <summary>
        /// Scope
        /// </summary>
        public List<string> Scope { get; set; }
        /// <summary>
        /// Name Scope
        /// </summary>
        public List<string> NamedScope { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public void PersonaContextDTO()
        {
            Title = "Anon";
            ParentalAccessEnabled = false;
            StaffAccessEnabled = false;
            StudentAccessEnabled = false;

            Scope = new List<string>();
            NamedScope = new List<string>();
        }
    }
}
