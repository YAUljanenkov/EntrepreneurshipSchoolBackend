using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Tasks")]
    public class Task
    {
        [Key] public int Id { get; set; }

        public string? Link { get; set; }

        [Required] public DateTime Deadline { get; set; }

        [Required, StringLength(128)] public string Title { get; set;} = string.Empty;
        [StringLength(1024)] public string? Criteria { get; set; } = string.Empty;
        [StringLength(1024)] public string? Comment { get; set; } = string.Empty;
        [ForeignKey("LessonId")] public virtual Lesson? Lesson { get; set; }

        [ForeignKey("TaskTypeId"), Required] public TaskType? Type { get; set; }

        public bool? IsGroup { get; set; } = false;

    }
}
