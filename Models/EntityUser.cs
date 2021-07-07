using Microsoft.AspNetCore.Identity;

namespace SchoolManagerApi.Models
{
    public class EntityUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}