using System.Collections.Generic;

namespace ct._Core.Repo.Dto
{
    /// <summary>
    /// Inventory Item dto
    /// </summary>
    public class InventoryItemDto
    {
        /// <summary>
        /// Id
        /// </summary>
        /// <returns></returns>
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }
        /// <summary>
        /// Group id
        /// </summary>
        /// <value></value>
        public string GroupId { get; set; }
        /// <summary>
        /// Inventory type 
        /// </summary>
        /// <value></value>
        public InventoryTypeEnum InventoryType { get; set; }
        /// <summary>
        /// UOM
        /// </summary>
        /// <returns></returns>
        public string UOM { get; set; }
        /// <summary>
        /// Owner id
        /// </summary>
        /// <returns></returns>
        public string OwnerId { get; set; }
        /// <summary>
        /// Tags
        /// </summary>
        /// <returns></returns>
        public List<string> Tags { get; set; }
        /// <summary>
        /// Attributes
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> Attributes { get; set; }
        /// <summary>
        /// Status
        /// </summary>
        /// <returns></returns>
        public InventoryItemStatusEnum Status { get; set; }
    }

    /// <summary>
    /// Inventory item status enum
    /// </summary>
    public enum InventoryItemStatusEnum
    {
        /// <summary>
        /// Active
        /// </summary>
        Active = 1,
        /// <summary>
        /// Disabled
        /// </summary> 
        Disabled = 99
    }

    /// <summary>
    /// Inventory Type Enum
    /// </summary>
    public enum InventoryTypeEnum
    {
        /// Inventory
        Inventory = 1,
        /// Group
        Group = 2
    }
}