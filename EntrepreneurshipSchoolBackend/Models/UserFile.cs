using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("UserFiles")]
    public class UserFile
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(256)] public string Path { get; set; }
    }
}
