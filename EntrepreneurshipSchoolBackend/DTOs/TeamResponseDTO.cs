namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class TeamComplexResponse
    {
        public Pagination pagination { set; get; }
        public List<TeamInfo> content { set; get; }
    }
}
