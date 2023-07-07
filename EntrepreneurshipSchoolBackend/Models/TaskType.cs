using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class TaskType
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
    }
}
