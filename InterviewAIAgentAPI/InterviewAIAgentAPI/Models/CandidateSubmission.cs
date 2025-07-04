using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InterviewAIAgentAPI.Models
{
    public class CandidateSubmission
    {
        [Key]
        public Guid CandidateId { get; set; }
        public string CandidateFullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int TotalYearsExperience { get; set; }
        public string Description { get; set; } // Optional personal statement
        [NotMapped]
        public IFormFile CV { get; set; } // CV as uploaded file
        public List<string> Questions { get; set; }
    }
}
