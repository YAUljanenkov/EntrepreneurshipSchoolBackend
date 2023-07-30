namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class TeamInfo
    {
        public int id { set; get; }
        public int teamNumber { set; get; }
        public string projectTheme { set; get; }
    }

    public class ExtendedTeamInfo
    {
        public int id { set; get; }
        public int teamNumber { set; get; }
        public string projectTheme { set; get; }
        public List<AccountInfo> members { set; get; }
    }

    public class ShortenTeamInfo
    {
        public int id { set; get; }
        public int number { set; get; }
    }

    public class PublicTeammateInfo
    {
        public int id { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string messenger { get; set; }
        public string role { get; set; }

    }

    public class PublicTeamInfo
    {
        public int id { set; get; }
        public int teamNumber { set; get; }
        public string projectTheme { set; get; }
        public List<PublicTeammateInfo> members { set; get; }

    }
}
