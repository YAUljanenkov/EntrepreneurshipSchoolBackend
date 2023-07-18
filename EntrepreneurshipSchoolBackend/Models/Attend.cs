using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Attend
    {
        [Key] public int Id { get; set; }
        public int LearnerId { get; set; }
        public int LessonId { get; set; }
        [Required] public Learner? Learner { get; set;}
        [Required] public Lesson? Lesson { get; set; }

        public Transaction? Transaction { get; set; }

        [Required] public char DidCome { get; set; }

    }
}
