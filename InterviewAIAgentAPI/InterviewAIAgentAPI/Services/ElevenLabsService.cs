using InterviewAIAgentAPI.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace InterviewAIAgentAPI.Services
{
    public class ElevenLabsService
    {
        private readonly HttpClient _httpClient;
        private readonly ElevenlabSettings _apiKey;

        public ElevenLabsService(HttpClient httpClient, IOptions<ElevenlabSettings> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value;
        }

        public async Task<byte[]> TextToSpeechAsync(string text, string voiceId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey.ElevenlabAPIKey);
            request.Content = new StringContent(JsonSerializer.Serialize(new { text }), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> SpeechToTextAsync(byte[] audio)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.elevenlabs.io/v1/speech-to-text");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey.ElevenlabAPIKey);
            request.Content = new ByteArrayContent(audio);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("text").GetString();
        }
    }
}