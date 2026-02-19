using Microsoft.AspNetCore.Http;

namespace SafeBit.Api.DTOs.Menu
{
    public class MenuUploadRequest
    {
        public IFormFile File { get; set; } = null!;
        public string RestaurantName { get; set; } = null!;
    }
}
