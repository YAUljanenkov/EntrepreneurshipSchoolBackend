using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Claim
    {
        [Key] public int Id { get; set; }

        [Required] public string? Comment { get; set; } = string.Empty;

        [Required] public ClaimStatus? Status { get; set; }

        [Required] public float Sum { get; set; }

        [Required] public ClaimType? Type { get; set; }

        public Lot? Lot { get; set; }

        public Learner? Learner { get; set; }

        public Task? Task { get; set; }

        public Learner? Receiver { get; set; }

    }
}
