namespace EntrepreneurshipSchoolBackend.DTOs;

public record ClaimLotDTO(string name, string description, string terms, int price);
public record CreateClaimDTO(string claimType, ClaimLotDTO? lot, int? buyingLotId, int? receiverId, int? sum);