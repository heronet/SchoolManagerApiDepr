using System;

namespace SchoolManagerApi.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public bool Delivered { get; set; } = false;
        public long ItemsCount { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public string UserId { get; set; }
        public EntityUser User { get; set; }
    }
}