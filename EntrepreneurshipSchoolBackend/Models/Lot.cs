using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Lot
    {
        [Key] public int Id { get; set; }
        [Required] public string Description { get; set; } = string.Empty;
        [Required] public int Price { get; set; }
        [Required] public string Terms { get; set; } = string.Empty;
        public string? Performer { get; set; }
        public Learner? Learner { get; set; }
    }
}
