namespace SafeBit.Api.DTOs.AiAgent
{
    public class UserProfileDto
    {
        public List<string> Allergies { get; set; } = new();
        public List<string> Diseases { get; set; } = new();
        public bool? IsPregnant { get; set; }
    }

}
