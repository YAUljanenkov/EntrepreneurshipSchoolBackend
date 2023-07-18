using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Task
    {
        [Key] public int Id { get; set; }

        public string? Link { get; set; }

        [Required] public DateTime Deadline { get; set; }

        [Required] public string Title { get; set;} = string.Empty;
        public string? Criteria { get; set; } = string.Empty;
        public string? Comment { get; set; } = string.Empty;
        public Lesson? Lesson { get; set; }

        [Required] public TaskType? Type { get; set; }

        public bool? IsGroup { get; set; } = false;

    }
}
