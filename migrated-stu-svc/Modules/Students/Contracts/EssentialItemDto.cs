using System;
using System.Collections.Generic;
using Repo.Resource;

namespace Repo.Dto
{
    public class CreateItemRecordDto
    {
        public string StudentId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class UpdateItemDto
    {
        public string Id { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class DisableItemDto
    {
        public string Id { get; set; }
        public int ItemId { get; set; }
    }

    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string AmountAsString { get { return Amount.ToString("0.00"); } }
        public string InstId { get; set; }
        public ItemStatusEnum Status { get; set; }
    }

    public class StudentEssentialsDto
    {
        public string Id { get; set; }
        public string StudentId { get; set; }
        public List<ItemDto> Items { get; set; }
        public StudentEssentialsDto()
        {
            Items = new List<ItemDto>();
        }
    }


}