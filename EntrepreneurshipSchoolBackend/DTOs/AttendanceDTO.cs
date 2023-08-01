using System.ComponentModel.DataAnnotations;

namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class AttendanceDTO
    {
        public LessonAttendancyDTO lesson { get; set; }
        public List<LearnerAttendDTO> learners { get; set; }
    }

    public class UpdateAttendancyDTO
    {
        public class UpdateAttendancyLearner
        {
            public int Id { get; set; }

            public char DidCome { get; set; }
            public int AccruedCurrency { get; set; }
        }

        [Required(ErrorMessage = "Some of the crucial properties have not been specified")]
        public int LessonId { get; set; }
        public List<UpdateAttendancyLearner> learners { get; set; }
    }
}
