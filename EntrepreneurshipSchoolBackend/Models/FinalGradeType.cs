using EntrepreneurshipSchoolBackend.Utility;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    /// <summary>
    /// Класс-модель всех весов к финальной оценке. Предполагается пять записей:
    /// 1. Вес оценки за ДЗ;
    /// 2. Вес оценки за тесты;
    /// 3. Вес оценки за конкурсы;
    /// 4. Вес оценки за экзамены;
    /// 5. Вес оценки за посещение.
    /// </summary>
    [Table("FinalTypes")]
    public class FinalGradeType : IType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }

        [Required] public string Name { get; set; } = string.Empty;

        [Required] public double Weight { get; set; } = 0;
    }
}
