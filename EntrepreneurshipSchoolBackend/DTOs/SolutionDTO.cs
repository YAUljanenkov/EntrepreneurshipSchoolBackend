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
}
