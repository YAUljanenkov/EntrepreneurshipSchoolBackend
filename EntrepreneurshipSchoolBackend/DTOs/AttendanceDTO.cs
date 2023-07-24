namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class AttendanceDTO
    {
        public LessonAttendancyDTO lesson { get; set; }
        public List<LearnerAttendDTO> learners { get; set; }
    }
}
