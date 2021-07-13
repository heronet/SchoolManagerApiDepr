using System;
using System.Collections.Generic;

namespace SchoolManagerApi.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public double Price { get; set; }
        public long Stock { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ThumbnailId { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}