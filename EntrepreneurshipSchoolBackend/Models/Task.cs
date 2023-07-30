using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Tasks")]
    public class Task
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }

        public string? Link { get; set; }

        [Required] public DateTime Deadline { get; set; }

        [Required, StringLength(128)] public string Title { get; set;} = string.Empty;
        [StringLength(1024)] public string? Criteria { get; set; } = string.Empty;
        [StringLength(1024)] public string? Comment { get; set; } = string.Empty;
        public Lesson? Lesson { get; set; }

        [Required] public TaskType Type { get; set; }

        public bool? IsGroup { get; set; } = false;

    }
}
