using SafeBit.Api.DTOs.Chat;
using SafeBit.Api.DTOs.Menu;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SafeBit.Api.Services
{
    
    public class AiAgentService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AiAgentService(HttpClient http, IConfiguration configuration)
        {
            _http = http;

            var baseUrl = configuration["AiAgent:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("Missing AiAgent:BaseUrl configuration.");
            }

            _http.BaseAddress = new Uri(baseUrl);
            _http.Timeout = TimeSpan.FromSeconds(60);
        }


        // Analyzes a menu image and returns structured data
        public async Task<AiAnalyzeMenuResponse> AnalyzeMenuAsync(
            IFormFile menuImage,
            AiUserProfileDto profile)
        {
            using var form = new MultipartFormDataContent();

            var imageContent = new StreamContent(menuImage.OpenReadStream());
            var contentType = string.IsNullOrWhiteSpace(menuImage.ContentType)
                ? "application/octet-stream"
                : menuImage.ContentType;
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(imageContent, "file", menuImage.FileName);

            var profileJson = JsonSerializer.Serialize(profile);
            form.Add(
                new StringContent(profileJson, Encoding.UTF8, "application/json"),
                "user_profile_json");

            var response = await _http.PostAsync("/analyze-menu", form); // call the AI agent endpoint

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AiAnalyzeMenuResponse>(json, _jsonOptions);

            if (result == null)
                throw new HttpRequestException("AI response could not be parsed.");

            return result;
        }

        public async Task<AiChatPythonResponseDto> ChatAsync(AiChatRequestDto request)
        {
            var response = await _http.PostAsJsonAsync("/chat", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(error);
            }

            var result = await response.Content.ReadFromJsonAsync<AiChatPythonResponseDto>(_jsonOptions);

            if (result == null)
                throw new HttpRequestException("AI chat response could not be parsed.");

            return result;
        }
    }
}
