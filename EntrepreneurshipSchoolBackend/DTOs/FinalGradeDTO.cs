using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class FinalAssessmentDTO
    {
        public double finalAssessment { get; set; }
        public string type { get; set; }
    }

    public class FinalGradeDTO
    {
        public List<FinalAssessmentDTO> assessments { get; set; }

        public double bonus { get; set; }
        public double total { get; set; }
    }

    public class FinalWeightDTO
    {
        public double weight { get; set; }
        public string type { get; set; }
    }

    public class FinalBonusDTO
    {
        [Required] public int learnerId { get; set;}
        [Required] public double bonus { get; set; }
    }
}
