using System;
using System.Net;

namespace ct._Core.Security
{
    /// <summary>
    ///     Field names are shortened to save space.
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public AuditLog()
        {
            Id = Guid.NewGuid().ToString();
            In = DateTime.UtcNow;
        }
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        ///     AppId
        /// </summary>
        public string App { get; set; }

        /// <summary>
        ///     User Email
        /// </summary>
        public string UE { get; set; }

        /// <summary>
        ///     Session Id
        /// </summary>
        public string SId { get; set; }
        /// <summary>
        /// Request URL
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        ///     Ip address
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Intime
        /// </summary>
        public DateTime In { get; set; }
        /// <summary>
        /// Out time
        /// </summary>
        public DateTime Out { get; set; }
        /// <summary>
        /// Http Status Code
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Error / Other messages if any
        /// </summary>
        public string Message { get; set; }
    }
}