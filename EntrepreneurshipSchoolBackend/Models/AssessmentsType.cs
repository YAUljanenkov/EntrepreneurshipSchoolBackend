using EntrepreneurshipSchoolBackend.Utility;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("AssessmentsTypes")]
    public class AssessmentsType : IType
    {
        [Key] public int Id { get; set; }
        [Required] public string Name { get; set; } = string.Empty;
    }
}
