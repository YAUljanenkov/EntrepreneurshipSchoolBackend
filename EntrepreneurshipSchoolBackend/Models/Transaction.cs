using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }
        [Required, StringLength(512)] public string Comment { get; set; } = String.Empty;
        [Required] public DateTime Date { get; set; }
        [Required] public int Sum { get; set; } 
        [Required] public TransactionType Type { set; get; }
        [Required] public Learner Learner { set; get; }
        public Claim? Claim { set; get; }

    }
}
