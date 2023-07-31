namespace EntrepreneurshipSchoolBackend.DTOs
{
    /// <summary>
    /// Подробное описание ученика.
    /// </summary>
    public class LearnerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<int> TeamNumber { get; set; }
        public string Role { get; set; }
        public int Balance { get; set; }
    }

    /// <summary>
    /// Краткое описание ученика.
    /// </summary>
    public class LearnerShortDTO
    {
        public int Id { get; set; }
        // Чаще всего имя слагается из фамилии и имени.
        public string Name { get; set; }
    }

    /// <summary>
    /// Описание ученика, использующееся в методах AttendanceController.
    /// </summary>
    public class LearnerAttendDTO
    {
        public LearnerDTO learner { get; set; }
        public bool didCome { get; set; }
        public int AccruedCurrency { get; set; }
    }
}
