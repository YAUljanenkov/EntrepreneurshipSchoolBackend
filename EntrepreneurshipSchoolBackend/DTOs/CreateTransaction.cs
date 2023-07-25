namespace EntrepreneurshipSchoolBackend.DTOs;

public record CreateTransaction(int learnerId, string description, int sum);