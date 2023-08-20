using EntrepreneurshipSchoolBackend.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class AssessmentDTO
    {
        public int Id { get; set; }
        public LearnerShortDTO learner {get; set; }
        public TaskOutputDTO task { get; set; }
        public DateTime issueDate{ get; set; }
        public AssessmentsType assessmentType { get; set; }
        public int assessment { get; set; }
    }

    public class AssessmentPageDTO
    {
        public Pagination pagination { get; set; }
        public List<AssessmentDTO> content { get; set; }
    }

    /// <summary>
    /// Используется в методе, создающий оценки/assessments
    /// </summary>
    public class AssessmentInputDTO
    {
        [Required] public int id { get; set; }
        public int? learnerId { get; set; }
        [Required] public int assessment { get; set; }
        public int? taskId { get; set;}
        public string? comment { get; set; }
    }

    /// <summary>
    /// Используется для возврата информации об оценке по идентификатору.
    /// </summary>
    public class AssessmentByIdDTO
    {
        [Required] public int id { get; set; }
        [Required] public LearnerShortDTO learner { get; set; }
        public LearnerShortDTO? tracker { get; set; }
        [Required] public TaskOutputDTO task { get; set; }
        [Required] public DateTime issueDate { get; set; }
        [Required] public string AssessmentType { get; set; }
        [Required] public int assessment { get; set; }
        public string? comment { get; set; }
    }

    public class LearnerAssessmentDTO
    {
        public int id { get; set; }
        public string taskTitle { get; set; }
        public string taskType { get; set; }
        public int? lessonId { get; set; }
        public DateTime issueDate { get; set; }
        public int assessment { get; set; }
    }

    public class AssessmentTrackerDTO
    {
        public int id { get; set; }
        public string trackerName { get; set; }
        public int assessment { get; set; }
        public string comment { get; set; }
    }

}
