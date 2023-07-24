namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class LessonOutputDTO
    {
        public int Id { get; set; }
        public int Number { get; set; }
    }

    public class LessonAttendancyDTO
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public DateTime date { get; set; }
    }
}
