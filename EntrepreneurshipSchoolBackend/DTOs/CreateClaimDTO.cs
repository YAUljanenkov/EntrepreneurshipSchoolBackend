namespace EntrepreneurshipSchoolBackend.DTOs;

public record CreateClaimDTO
{
    public string claimType { get; }

    public Lot? lot { get; }
    public int? buyingLotId { get; }
    public int? receiverId { get; }
    public int? sum { get; }

    public record Lot(string name, string desciption, string terms, int price);
}