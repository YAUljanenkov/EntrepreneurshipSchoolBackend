using EntrepreneurshipSchoolBackend.Models;

namespace EntrepreneurshipSchoolBackend.DTOs;

public record class TransactionDTO
{
    public int id { get; }
    public  Learner learner { get; }
    public string type { get; }
    public string description { get; }
    public string dateTime { get; }
    public int sum { get; }
    public TransactionDTO(Transaction transaction)
    {
        this.id = transaction.Id;
        this.learner = new Learner(
            transaction.Learner.Id,
            $"{transaction.Learner.Name} {transaction.Learner.Surname}"
        );
        this.type = transaction.Type.Name;
        this.description = transaction.Comment;
        this.dateTime = transaction.Date.ToString("dd.MM.yyyy HH:mm:ss");
        this.sum = transaction.Sum;
    }

    public record Learner(int id, string name);
}