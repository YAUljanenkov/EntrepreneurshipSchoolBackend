namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class AccountInfo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public List<int> teamNumber { get; set; }
        public string role { get; set; }
        public int balance { get; set; }
    }

    public class ExtendedAccountInfo
    {
        public int id { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string? messenger { get; set; }
        public bool? gender { get; set; }
        public List<int> teamNumber { get; set; }
        public string role { get; set; }
        public int balance { get; set; }
    }

    public class AccountShortenInfo
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
