using InterviewAIAgentAPI.Models;
using InterviewAIAgentAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterviewAIAgentAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly GmailServiceHelper _gmailService;
        private readonly TwilioService _twilioService;

        public EmailController(GmailServiceHelper gmailService, TwilioService twilioService)
        {
            _gmailService = gmailService;
            _twilioService = twilioService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail(string to, string subject, string body)
        {
            await _gmailService.SendEmailAsync(to, subject, body);
            return Ok("Email sent!");
        }

        [HttpPost("send-interview-invite")]
        public async Task<IActionResult> SendInterviewInvite([FromBody] CandidateSubmission candidate)
        {
            // Generate Twilio video room link
            string videoRoomLink = _twilioService.GenerateVideoRoomLink(candidate.CandidateId);

            // Compose email body
            string body = $"Dear {candidate.CandidateFullName},\n\n" +
                          "You are invited to an interview. Please join using the following link:\n" +
                          $"{videoRoomLink}\n\n" +
                          "Best regards,\nInterview Team";

            // Send email
            await _gmailService.SendEmailAsync(candidate.Email, "Interview Invitation", body);

            return Ok("Email sent with video room link.");
        }
    }
}
