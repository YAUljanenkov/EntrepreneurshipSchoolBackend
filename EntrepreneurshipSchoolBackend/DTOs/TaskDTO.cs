namespace EntrepreneurshipSchoolBackend.DTOs
{
    /// <summary>
    /// Чтобы полностью соответствовать спецификации из API, я завёл специальные объекты.
    /// Именно в такое форме приложение будет принимать информацию о заданиях с фронта.
    /// 9 свойств; все могут принимать значение null. 
    /// Необходимые свойства проверяются на null в методах.
    /// </summary>
    public class TaskInputDTO
    {
        public int? id { get; set; }
        public string? title { get; set; }
        public int? lessonId { get; set; }
        public string? description { get; set; }
        public string? criteria { get; set; }
        public bool? isTeamWork { get; set; }
        public string? link { get; set; }
        public string? taskType { get; set; }
        public DateTime? deadline { get; set; }
    }

    public class TaskPageDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public LessonOutputDTO? Lesson { get; set; }
        public string TaskType { get; set; }
        public DateTime deadline { get; set; }
    }

    public class TaskOutputDTO
    {
        public int Id { get; set; }
        public string Title { get; set;}
    }

    public class TaskOutputPageDTO
    {
        public Pagination pagination { get; set; }
        public List<TaskPageDTO> content { get; set; }
    }

    public class TaskDeadlineDTO
    {
        public LessonOutputDTO? lesson { get; set; }
        public TaskOutputDTO task { get; set; }
        public DateTime deadline { get; set; }
    }
}
