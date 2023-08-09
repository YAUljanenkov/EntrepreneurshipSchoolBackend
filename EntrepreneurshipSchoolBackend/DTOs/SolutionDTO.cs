using EntrepreneurshipSchoolBackend.Models;

namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class SolutionListDTO
    {
        public Pagination? pagination { get; set; }
        public List<SolutionDTO> content { get; set; }
    }

    public class SolutionLearnerDTO
    {
        public TaskOutputDTO task { get; set;}
        public DateTime deadline { get; set; }
        public DateTime? completeDateTime { get; set; }
    }

    public class SolutionDTO
    {
        public LearnerShortDTO? learner { get; set; }
        public GroupDTO? team { get; set; }
        public DateTime completeDateTime { get; set; }
        public int fileId { get; set; }
    }

    public class GroupDTO
    {
        public int Id { get; set; }
        public int Number { get; set; }
    }

    public class SolutionByIdDTO
    {
        public int Id { get; set; }
        public TaskSolutionDTO task { get; set; }
        public DateTime completeDateTime { get; set; }
        public int fileId { get; set; }
        public List<AssessmentTrackerDTO> trackerAssessments { get; set; }
    }

    /// <summary>
    /// Объект, который используется для трекерского списка сданных решений.
    /// </summary>
    public class TrackerSeeSolutionDTO
    {
        public int id { get; set; }
        public string Name { get; set; }
        public DateTime? completedDateTime { get; set; }
        public int? assessment { get; set; }
    }

    /// <summary>
    /// Объект, который используется для просмотра подробной информации о решении трекером.
    /// </summary>
    public class TrackerInspectSolutionDTO
    {
        public int id { get; set; }
        public LearnerShortDTO learner { get; set; }
        public ShortenTeamInfo team { get; set; }

        public TaskSolutionDTO task { get; set; }

        public DateTime completeDateTime { get; set; }

        public int fileId { get; set; }
        public string? comment { get; set; }
        public int? assessment { get; set; }
    }
}
