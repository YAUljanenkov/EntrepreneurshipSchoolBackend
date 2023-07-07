using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Lesson
    {
        [Key] public int Id { get; set; }

        [Required] public string Title { get; set; } = String.Empty;

        [Required] public string Description { get; set; } = String.Empty;

        [Required] public DateTime Date { get; set; }

        [Required] public string PresLink { get; set; } = String.Empty;

        [Required] public string VideoLink { get; set; } = String.Empty;
    }
}
