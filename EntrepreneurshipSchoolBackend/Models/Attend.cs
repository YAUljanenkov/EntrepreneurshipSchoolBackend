using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Attends")]
    [PrimaryKey(nameof(LearnerId), nameof(LessonId))]
    public class Attend
    {
        public int LearnerId { get; set; }
        public int LessonId { get; set; }
        [Required] public Learner? Learner { get; set;}
        [Required] public Lesson? Lesson { get; set; }

        public Transaction? Transaction { get; set; }

        [Required] public char DidCome { get; set; }

    }
}
