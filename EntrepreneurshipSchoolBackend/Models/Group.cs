using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Groups")]
    public class Group
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }

        [Required] public int Number { get; set; }

        [Required, StringLength(100)] public string Name { get; set; }

        [Required] public string Theme { get; set; } = string.Empty;

        public virtual ICollection<Relate> Relate { get; set; }

    }
}
