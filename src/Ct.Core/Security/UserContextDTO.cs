using ct._Core.Security;
using Repo.Dto;
using System.Collections.Generic;

namespace ct._Core.Security
{
    /// <summary>
    /// User Context Dto
    /// </summary>
    public class UserContextDto
    {
        /// <summary>
        /// App Id
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// User Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// User First Name
        /// </summary>
        public string UserFirstName { get; set; }
        /// <summary>
        /// User Middle Name
        /// </summary>
        public string UserMiddleName { get; set; }
        /// <summary>
        /// User Last Name
        /// </summary>
        public string UserLastName { get; set; }
        /// <summary>
        /// User Email
        /// </summary>
        public string UserEmail { get; set; }
        /// <summary>
        /// Photo Url
        /// </summary>
        public string PhotoURL { get; set; }
        /// <summary>
        /// Force Change Password
        /// </summary>
        public bool ForceChangePassword { get; set; }
        /// <summary>
        /// Org Url
        /// </summary>
        public string OrgLogoUrl { get; set; }
        /// <summary>
        /// Is Root Admin
        /// </summary>
        public bool IsRootAdmin { get; set; }
        /// <summary>
        /// Current Persona
        /// </summary>
        public PersonaContextDto CurrentPersona { get; set; }
        /// <summary>
        /// Named Scope
        /// </summary>
        public List<string> NamedScope { get; set; }
        /// <summary>
        /// Scope
        /// </summary>
        public List<string> Scope { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        public UserContextDto()
        {
            Scope = new List<string>();
            NamedScope = new List<string>();
            CurrentPersona = new PersonaContextDto();
        }
    }
}
