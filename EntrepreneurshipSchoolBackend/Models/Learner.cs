using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Learners")]
    public class Learner
    {
        [Key] public int Id { get; set; }

        [Required, StringLength(64)] public string Name { get; set; } = String.Empty;

        [Required, StringLength(64)] public string Surname { get; set; } = String.Empty;

        [StringLength(64)] public string? Lastname { get; set; }

        [StringLength(64)] public string? Messenger { get; set; }

        public char? Gender { get; set; }

        [Required, StringLength(128)] public string EmailLogin { get; set; } = String.Empty;
         
        [Required, StringLength(256)] public string? Password { get; set; } = String.Empty;

        [Required] public char IsTracker { get; set; }

        [Required, StringLength(12)] public string Phone { get; set; } = String.Empty;

        [Required] public int Balance { get; set; }

        [Required] public double ResultGrade { get; set; }

        public double? GradeBonus { get; set; }

        public virtual ICollection<Relate> Relate { get; set; }
    }
}
