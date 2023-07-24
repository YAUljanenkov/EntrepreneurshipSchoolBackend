namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class LearnerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IEnumerable<int> TeamNumber { get; set; }
        public string Role { get; set; }
        public int Balance { get; set; }
    }

    public class LearnerAttendDTO
    {
        public LearnerDTO learner { get; set; }
        public bool didCome { get; set; }
        public int AccruedCurrency { get; set; }
    }
}
