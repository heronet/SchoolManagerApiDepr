using System.Collections.Generic;

namespace SchoolManagerApi.DTOs
{
    public class ModifyRoleClaimsDTO
    {
        public string Name { get; set; }
        public string Mode { get; set; } // Remove or Add
        public List<string> Permissions { get; set; }
    }
}