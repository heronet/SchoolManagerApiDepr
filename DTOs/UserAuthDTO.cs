using System.Collections.Generic;

namespace SchoolManagerApi.DTOs
{
    public class UserAuthDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Claims { get; set; }
    }
}