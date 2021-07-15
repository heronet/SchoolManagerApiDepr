using System.Collections.Generic;

namespace SchoolManagerApi.DTOs
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public long Count { get; set; }
    }
}