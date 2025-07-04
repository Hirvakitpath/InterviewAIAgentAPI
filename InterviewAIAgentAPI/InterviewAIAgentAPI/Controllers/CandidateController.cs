using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using InterviewAIAgentAPI.Models;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using InterviewAIAgentAPI.Data;

namespace InterviewAIAgentAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _dbContext;

        public CandidateController(IConfiguration configuration, IHttpClientFactory httpClientFactory, ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _dbContext = dbContext;
        }

        [HttpPost("add-candidate-with-cv")]
        public async Task<IActionResult> AddCandidateWithCV([FromForm] CandidateSubmission candidateSubmission)
        {
            try
            {
                if (candidateSubmission == null || candidateSubmission.CV == null || candidateSubmission.CV.Length == 0)
                {
                    return BadRequest("Candidate submission or CV file is required.");
                }

                candidateSubmission.CandidateId = Guid.NewGuid();

                // Read CV file into memory
                using (var memoryStream = new MemoryStream())
                {
                    await candidateSubmission.CV.CopyToAsync(memoryStream);
                    candidateSubmission.CVData = memoryStream.ToArray();
                }

                // Extract text from PDF
                string cvContent;
                try
                {
                    cvContent = await ExtractTextFromPdf(candidateSubmission.CVData);
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error extracting text from PDF: {ex.Message}");
                }

                // Get questions from OpenAI
                List<string> questions;
                try
                {
                    questions = await GetQuestionsFromOpenAI(cvContent, candidateSubmission.Description);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error generating questions from OpenAI: {ex.Message}");
                }

                // Assign questions to candidate
                candidateSubmission.Questions = questions;

                // Remove CV file from model before saving to DB
                candidateSubmission.CV = null;

                // Save candidate to DB
                try
                {
                    _dbContext.Candidates.Add(candidateSubmission);
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error saving candidate to database: {ex.Message}");
                }

                return Ok(new
                {
                    candidateSubmission.CandidateId,
                    candidateSubmission.CandidateFullName,
                    candidateSubmission.Questions,
                    Message = "Candidate added successfully with questions."
                });
            }
            catch (Exception ex)
            {
                // Log the exception (you should use your logging framework here)
                // _logger.LogError(ex, "Unexpected error in AddCandidateWithCV");

                return StatusCode(500, "An unexpected error occurred while processing the candidate submission.");
            }
        }

        private async Task<string> ExtractTextFromPdf(byte[] pdfData)
        {
            var text = new StringBuilder();
            try
            {
                using (var pdfReader = new PdfReader(pdfData))
                {
                    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                    {
                        text.Append(PdfTextExtractor.GetTextFromPage(pdfReader, page));
                    }
                }
                return await Task.FromResult(text.ToString());
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., invalid PDF)
                return await Task.FromResult($"Error extracting text from PDF: {ex.Message}");
            }
        }

        private async Task<List<string>> GetQuestionsFromOpenAI(string cvContent, string description)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            // Build the prompt dynamically based on whether description is provided
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are an expert technical interviewer and resume parser.");
            promptBuilder.AppendLine("Given the following CV content" + (string.IsNullOrWhiteSpace(description) ? "" : " and candidate description") + ", first determine the candidate's total years of professional experience from the CV.");
            promptBuilder.AppendLine("Then, based on the extracted years of experience, the CV content" + (string.IsNullOrWhiteSpace(description) ? "" : ", and the description") + ", generate exactly 5 technical interview questions as a JSON array under the property 'Questions'.");
            promptBuilder.AppendLine("Ensure that each question covers a different technology, framework, or skill area mentioned in the CV (do not focus all questions on a single technology).");
            promptBuilder.AppendLine("Return only the JSON object with the 'Questions' property. Do not include the years of experience or any other properties in the output.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("CV Content:");
            promptBuilder.AppendLine(cvContent);
            if (!string.IsNullOrWhiteSpace(description))
            {
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Description:");
                promptBuilder.AppendLine(description);
            }

            var prompt = promptBuilder.ToString();

            var requestBody = new
            {
                model = "gpt-4.1-mini-2025-04-14",
                messages = new[]
                {
            new { role = "system", content = "You extract structured data from resumes and generate technical interview questions." },
            new { role = "user", content = prompt }
        }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var choices = jsonResponse.GetProperty("choices").EnumerateArray();
            var content = choices.First().GetProperty("message").GetProperty("content").GetString();

            // Extract JSON from response (handle code blocks)
            var jsonString = ExtractJsonFromOpenAIResponse(content);

            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            var questions = root.GetProperty("Questions")
                .EnumerateArray()
                .Select(x => x.GetString())
                .Where(x => x != null)
                .ToList();

            return questions;
        }

        // Helper to extract JSON from OpenAI response
        private string ExtractJsonFromOpenAIResponse(string content)
        {
            var start = content.IndexOf('{');
            var end = content.LastIndexOf('}');
            if (start >= 0 && end > start)
                return content.Substring(start, end - start + 1);
            return content;
        }
    }
}