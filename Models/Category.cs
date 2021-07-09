using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagerApi.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}