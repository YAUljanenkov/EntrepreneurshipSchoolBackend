namespace EntrepreneurshipSchoolBackend.Utility
{
    /// <summary>
    /// Интерфейс для моделей-типов, вроде TaskType, TransactionType и т. д.
    /// </summary>
    public interface IType
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
