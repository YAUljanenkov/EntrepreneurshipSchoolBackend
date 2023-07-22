using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Assessments")]
    public class Assessments
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }
        [ForeignKey("Learner")] public int LearnerId { get; set; }
        [ForeignKey("Task")] public int TaskId { get; set; }

        [Required] public Learner? Learner { get; set; }
        [Required] public Task? Task { get; set; }

        [Required] public int Grade { get; set; }

        [Required] public DateTime Date { get; set; }

        public string Comment { get; set; } = string.Empty;

        [Required] public int? AssessmentsType { get; set; }

        [ForeignKey("Tracker")] public int TrackerId { get; set; }

        public Learner? Tracker { get; set; }
    }
}
