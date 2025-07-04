using InterviewAIAgentAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewAIAgentAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<CandidateResume> Candidates { get; set; }
        //public DbSet<Interview> Interviews { get; set; }
        //public DbSet<Question> Questions { get; set; }

    }
}
