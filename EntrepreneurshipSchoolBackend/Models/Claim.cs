using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Claims")]
    public class Claim
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }

        [Required, StringLength(100)] public string? Title { get; set; } = string.Empty;

        [Required, StringLength(1000)] public string? Description { get; set; } = string.Empty;

        [Required] public DateTime Date { get; set; }

        [Required] public ClaimStatus? Status { get; set; }

        public int? Sum { get; set; }

        [Required] public ClaimType? Type { get; set; }

        public Lot? Lot { get; set; }

        public Learner? Learner { get; set; }

        public Task? Task { get; set; }

        public Learner? Receiver { get; set; }

    }
}
