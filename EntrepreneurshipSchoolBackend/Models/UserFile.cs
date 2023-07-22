using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("UserFiles")]
    public class UserFile
    {
        [Key, StringLength(128)] public String Name { get; set; }
        [Required, StringLength(128)] public String FileType { get; set; }
    }
}
