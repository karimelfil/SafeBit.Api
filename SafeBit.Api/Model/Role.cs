using System.ComponentModel.DataAnnotations;

namespace SafeBit.Api.Model
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; } // Primary Key

        [Required]
        public string Type { get; set; } // "Admin" or "User"
    }
}
