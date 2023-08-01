namespace EntrepreneurshipSchoolBackend.DTOs
{

    public class BalanceNameResponse
    {
        public int balance { get; set; }
        public string name { get; set; }
    }

    public class AccountComplexResponse
    {
        public Pagination pagination { get; set; }
        public List<AccountInfo> content { get; set; }
    }
}
