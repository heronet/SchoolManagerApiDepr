using System;

namespace SchoolManagerApi.DTOs
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public Guid CategoryId { get; set; }
        public double Price { get; set; }
        public string ThumbnailUrl { get; set; }
        public string ThumbnailId { get; set; }
    }
}