using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Group
    {
        [Key] public int Id { get; set; }

        [Required] public string Theme { get; set; } = string.Empty;

    }
}
