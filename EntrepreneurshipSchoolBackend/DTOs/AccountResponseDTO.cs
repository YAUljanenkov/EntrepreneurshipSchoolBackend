namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class Pagination
    {
        public int page_number { get; set; }
        public int pageSize { get; set; }
        public int total_pages { get; set; }
        public int total_elements { get; set; }
    }

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
