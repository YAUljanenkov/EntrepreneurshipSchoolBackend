using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models;

public class Admin
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }
    [Required, StringLength(128)] public string EmailLogin { get; set; } = String.Empty;
    [Required, StringLength(256)] public string? Password { get; set; } = String.Empty;
}