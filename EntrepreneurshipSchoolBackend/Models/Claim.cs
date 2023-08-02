using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Claims")]
    public class Claim
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }

        [Required] public DateTime Date { get; set; }

        [Required] public ClaimStatus Status { get; set; }

        public int? Sum { get; set; }

        [Required] public ClaimType Type { get; set; }

        public Lot? Lot { get; set; }

        [Required] public Learner? Learner { get; set; }

        public Task? Task { get; set; }
        [StringLength(128)] public string? Title { get; set; }

        [StringLength(512)] public string? Description { get; set; }
        [StringLength(512)] public string? Terms { get; set; }
        [StringLength(128)] public string? Performer { get; set; }

        public Learner? Receiver { get; set; }

    }
}
