using System.ComponentModel.DataAnnotations;

namespace SchoolManagerApi.DTOs
{
    public class AddRoleDTO
    {
        [Required]
        public string Name { get; set; }
    }
}