using System.ComponentModel.DataAnnotations;

namespace SafeBit.Api.Model
{
    public class Disease
    {
        [Key]
        public int DiseaseID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Category { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}
