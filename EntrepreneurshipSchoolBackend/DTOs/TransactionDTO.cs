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
    public Claim? claim { get; }
    public TransactionDTO(Transaction? transaction)
    {
        if (transaction == null)
        {
            return;
        }
        this.id = transaction.Id;
        this.learner = new Learner(
            transaction.Learner.Id,
            $"{transaction.Learner.Name} {transaction.Learner.Surname}"
        );
        this.type = transaction.Type.Name;
        this.description = transaction.Comment;
        this.dateTime = transaction.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
        this.sum = transaction.Sum;
        if (transaction.Claim != null)
        {
            claim = new Claim(transaction.Claim.Id, claimTypeToHuman(transaction.Claim));
        }
    }

    public record Learner(int id, string name);
    public record Claim(int id, string title);

    private string claimTypeToHuman(Models.Claim claim)
    {
        return claim.Type.Name switch
        {
            "BuyingLot" => $"Покупка лота №{claim.Lot.Id} учеником {claim.Receiver.Surname} {claim.Receiver.Name} {claim.Receiver.Lastname}. Дата: {claim.Date:dd.MM.yyyy HH:mm:ss}",
            "FailedDeadline" => $"Просроченный дедлайн задания {claim.Task.Title} учеником {claim.Learner.Surname} {claim.Learner.Name} {claim.Learner.Lastname}. Дата: {claim.Date:dd.MM.yyyy HH:mm:ss}",
            "Transfer" => $"Перевод от ученика {claim.Learner.Surname} {claim.Learner.Name} {claim.Learner.Lastname} ученику {claim.Receiver.Surname} {claim.Receiver.Name} {claim.Receiver.Lastname} {claim.Sum} шпрот. Дата: {claim.Date:dd.MM.yyyy HH:mm:ss}",
            "PlacingLot" => $"Продажа лота №{claim.Lot.Id} учеником {claim.Learner.Surname} {claim.Learner.Name} {claim.Learner.Lastname}. Дата: {claim.Date:dd.MM.yyyy HH:mm:ss}",
            _ => ""
        };
    }
}