namespace EntrepreneurshipSchoolBackend.DTOs;

public record UpdateLotDTO(int id, string? title, string? description, string? terms, int? price, string? performer);