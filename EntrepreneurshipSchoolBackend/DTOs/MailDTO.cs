namespace EntrepreneurshipSchoolBackend.DTOs;

/// <summary>
/// E-mail object.
/// </summary>
/// <param name="grouping">Enum: All, Learners, Trackers</param>
/// <param name="learnersIds">Ids of specific learners</param>
/// <param name="teamsIds">Ids of specific teams</param>
/// <param name="title">A title of a mail.</param>
/// <param name="content">A content of a mail.</param>
public record MailDTO(List<string> grouping, List<int> learnersIds, List<int> teamsIds, string title, string content);