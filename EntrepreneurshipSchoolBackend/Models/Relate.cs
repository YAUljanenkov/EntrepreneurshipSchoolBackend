using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Relate
    {
        [Key] public int Id { get; set; }
        public int LearnerId { get; set; }
        public int GroupId { get; set; }
        [Required] public Learner? Learner { get; set; }
        [Required] public Group? Group { get; set; }

    }
}
