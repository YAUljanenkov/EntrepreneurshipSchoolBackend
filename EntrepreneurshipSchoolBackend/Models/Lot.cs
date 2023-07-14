using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Lots")]
    public class Lot
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }

        [Required, StringLength(128)] public string Title { get; set; } = string.Empty;

        [Required, StringLength(512)] public string Description { get; set; } = string.Empty;

        [Required] public int Number { get; set; }
        [Required] public int Price { get; set; }
        [Required, StringLength(512)] public string Terms { get; set; } = string.Empty;
        [StringLength(128)] public string? Performer { get; set; }
        public Learner? Learner { get; set; }
    }
}
