namespace EntrepreneurshipSchoolBackend.DTOs
{
    /// <summary>
    /// Класс нумерации объектов по страницам.
    /// </summary>
    public class Pagination
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalElements { get; set; }
    }
}
