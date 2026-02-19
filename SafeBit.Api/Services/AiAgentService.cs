using SafeBit.Api.DTOs.Menu;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SafeBit.Api.Services
{
    // Communicates with the AI agent to analyze menu images + user profiles.
    public class AiAgentService
    {
        private readonly HttpClient _http;

        public AiAgentService(HttpClient http)
        {
            _http = http;
            // Prefer configuration in Program.cs; keep this for now if you want.
            _http.BaseAddress = new Uri("http://192.168.18.10:8001");
            _http.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<AiAnalyzeMenuResponse> AnalyzeMenuAsync(
            IFormFile menuImage,
            AiUserProfileDto profile)
        {
            using var form = new MultipartFormDataContent();

            var imageContent = new StreamContent(menuImage.OpenReadStream());
            imageContent.Headers.ContentType =
                new MediaTypeHeaderValue(menuImage.ContentType);
            form.Add(imageContent, "file", menuImage.FileName);

            var profileJson = JsonSerializer.Serialize(profile);
            form.Add(new StringContent(profileJson, Encoding.UTF8), "user_profile_json");

            var response = await _http.PostAsync("/analyze-menu", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AiAnalyzeMenuResponse>(json);

            if (result == null)
                throw new HttpRequestException("AI response could not be parsed.");

            return result;
        }
    }
}
