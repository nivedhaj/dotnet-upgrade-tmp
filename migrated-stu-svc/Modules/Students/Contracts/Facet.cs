using System.Collections.Generic;

namespace Repo.Dto
{
    /// <summary>
    /// Facet
    /// </summary>
    public class Facet
    {
        /// <summary>
        /// Id
        /// </summary>
        public string ElemId { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Edit - Defaults to False
        /// </summary>
        public bool Edit { get; set; }
        /// <summary>
        /// Children
        /// </summary>
        public List<Facet> Children { get; set; }
        /// <summary>
        /// Ctor
        /// </summary>
        public Facet()
        {
            Children = new List<Facet>();
        }
    }

}
