namespace EntrepreneurshipSchoolBackend.DTOs
{
    /// <summary>
    /// Краткое описание урока из идентификатора и номера.
    /// </summary>
    public class LessonOutputDTO
    {
        public int Id { get; set; }
        public int Number { get; set; }
    }

    /// <summary>
    /// Объект, представляющий собой урок в методах AttendanceController.
    /// </summary>
    public class LessonAttendancyDTO
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public DateTime date { get; set; }
    }
}
