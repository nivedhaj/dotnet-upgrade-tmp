using System.Collections.Generic;

namespace Repo.Dto
{
    /// <summary>
    /// Item
    /// </summary>
    public class LineItemDto
    {
        /// <summary>
        /// Inventory item id
        /// </summary>
        /// <returns></returns>
        public string inventory_item_id { get; set; }
        /// <summary>
        /// Inventory item tracking ids
        /// </summary>
        /// <returns></returns>
        public List<string> tracking_ids { get; set; }
        /// <summary>
        /// Rate per unit
        /// </summary>
        /// <returns></returns>
        public decimal rate { get; set; }
        /// <summary>
        /// Quantity
        /// </summary>
        /// <returns></returns>
        public decimal quantity { get; set; }
    }
}