using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Lessons")]
    public class Lesson
    {
        [Key] public int Id { get; set; }

        [Required] public int Number { get; set; }

        [Required, StringLength(128)] public string Title { get; set; } = String.Empty;

        [Required, StringLength(1000)] public string Description { get; set; } = String.Empty;

        [Required] public DateTime Date { get; set; }

        [Required, StringLength(256)] public string PresLink { get; set; } = String.Empty;

        [Required, StringLength(256)] public string VideoLink { get; set; } = String.Empty;
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
