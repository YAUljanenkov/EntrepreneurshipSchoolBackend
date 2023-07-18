using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Assessments
    {
        [Key] public int Id { get; set; }
        [ForeignKey("Learner")] public int LearnerId { get; set; }
        [ForeignKey("Task")] public int TaskId { get; set; }

        [Required] public Learner? Learner { get; set; }
        [Required] public Task? Task { get; set; }

        [Required] public int Grade { get; set; }

        public string Comment { get; set; } = string.Empty;

        [Required] public AssessmentsType? AssessmentsType { get; set; }

        [ForeignKey("Tracker")] public int TrackerId { get; set; }

        public Learner? Tracker { get; set; }
    }
}
