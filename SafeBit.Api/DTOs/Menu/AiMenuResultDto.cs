namespace SafeBit.Api.DTOs.Menu
{
    public class AiMenuResultDto
    {
        public List<SafeDishDto> Safe_Dishes { get; set; }
        public List<UnsafeDishDto> Unsafe_Dishes { get; set; }
        public List<string> Recommendations { get; set; }
    }
}
