namespace EntrepreneurshipSchoolBackend.DTOs;

public record LotDTO(string name, string description, string terms, int price);
public record CreateClaimDTO(string claimType, LotDTO? lot, int? buyingLotId, int? receiverId, int? sum);