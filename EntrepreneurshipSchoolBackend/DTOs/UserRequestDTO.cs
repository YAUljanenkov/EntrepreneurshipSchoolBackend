namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class UserRequest
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string? middleName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string? messenger { get; set; }
        public bool? gender { get; set; }
        public string role { get; set; }
        public string password { get; set; }
    }

    public class AccountsComplexRequest
    {
        public string? name { get; set; }
        public string? email { get; set; }
        public int? team { get; set; }
        public string? role { get; set; }

        public string? sortProperty { get; set; }
        public string? sortOrder { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}
