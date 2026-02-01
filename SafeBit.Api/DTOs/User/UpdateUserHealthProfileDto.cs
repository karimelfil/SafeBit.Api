using System.ComponentModel.DataAnnotations;

namespace SafeBit.Api.DTOs.User
{
	public class UpdateUserHealthProfileDto
	{
		[Required]
		public int UserId { get; set; }

		public List<int> AllergyIds { get; set; } = new();

		public List<int> DiseaseIds { get; set; } = new();
	}
}
