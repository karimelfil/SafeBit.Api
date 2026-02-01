namespace SafeBit.Api.DTOs.User
{
	public class UserHealthInfoDto
	{
		public int UserId { get; set; }
		public List<HealthItemDto> Allergies { get; set; } = new();
		public List<HealthItemDto> Diseases { get; set; } = new();
	}
}
