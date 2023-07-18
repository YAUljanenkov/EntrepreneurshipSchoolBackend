using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Learner
    {
        [Key] public int Id { get; set; }

        [Required] public string Name { get; set; } = String.Empty;

        [Required] public string Surname { get; set; } = String.Empty;

        public string? Lastname { get; set; }

        [Required] public char Gender { get; set; }

        [Required] public string EmailLogin { get; set; } = String.Empty;
         
        [Required] public string Password { get; set; } = String.Empty;

        [Required] public char IsTracker { get; set; }

        [Required] public string Phone { get; set; } = String.Empty;

        [Required] public char Balance { get; set; }

        [Required] public float ResultGrade { get; set; }
    }
}
