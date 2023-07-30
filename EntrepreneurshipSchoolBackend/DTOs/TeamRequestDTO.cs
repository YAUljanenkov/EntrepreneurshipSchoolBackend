namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class TeamsComplexRequest
    {
        public int? teamNumber { set; get; }
        public string? sortProperty { set; get; }
        public string? sortOrder { set; get; }
        public int page { set; get; } = 1;
        public int pageSize { set; get; } = 10;
    }


    public class TeamRequest
    {
        public int? id { set; get; }
        public string projectTheme { set; get; }
        public List<int> members { set; get; }
    }


}
