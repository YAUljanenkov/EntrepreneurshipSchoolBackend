using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Solution
    {
        [Key] public int Id { get; set; }
        public int TaskId { get; set; }
        [Required] public Task? Task { get; set; }
        public int LearnerId { get; set; }
        public Learner? Learner { get; set; }
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        [Required] public DateTime CompleteDate { get; set; }

        [Required] public string File { get; set; } = string.Empty;
    }
}
