using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.Models
{
    public class Transaction
    {
        [Key] public int Id { get; set; }
        [Required] public string Comment { get; set; } = String.Empty;
        [Required] public DateTime Date { get; set; }
        [Required] public int Sum { get; set; } 
        [Required] public TransactionType? Type { set; get; }
        [Required] public Learner? Learner { set; get; }
        public Claim? Claim { set; get; }

    }
}
