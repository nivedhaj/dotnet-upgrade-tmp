using System;
using System.Collections.Generic;
using System.Linq;
using ct._Core.Repo.Resource;

namespace Repo.Resource
{
    /// <summary>
    /// Student Class
    /// </summary>
    public class StudentEssentials : Entity
    {
        public string StudentId { get; set; }
        public List<Item> Items { get; set; }
        public StudentEssentials()
        {
            Items = new List<Item>();
        }

        public void AddItem(string name, DateTime date, decimal amount, string instId)
        {
            var newId =  getItemId(this.Items);
            Items.Add( new Item { Id = newId, Name = name, Date = date, Amount = amount, InstId = instId, Status = ItemStatusEnum.Active  });
        }

        private int getItemId(List<Item> items)
        {
            if (items == null || items.Count == 0) return 1;
            int lastId = items.OrderByDescending(o => o.Id).Take(1).Select(o => o.Id).SingleOrDefault();
            return lastId + 1;
        }
    }

    public enum ItemStatusEnum
    {
        Active = 1,
        Disable = 99,
        Unknown = 0
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string InstId { get; set; }
        public ItemStatusEnum Status { get; set; }
    }
}