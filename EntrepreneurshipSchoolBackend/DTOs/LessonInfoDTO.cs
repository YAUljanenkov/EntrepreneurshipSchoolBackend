namespace EntrepreneurshipSchoolBackend.DTOs
{

    public class ShortenLessonInfo {
        public int id { set; get; }
        public int number { set; get; }
    }

    public class ShortenLesson
    {
        public int id { set; get; }
        public int number { set; get; }
        public string title { set; get; }
    }

    public class LessonInfo
    {
        public int id { set; get; }
        public int number { set; get; }
        public string title { set; get; }
        public string date { set; get; }
    }

    public class TaskInLesson
    {
        public int id {  get; set; }
        public string title { set; get; }
    }

    public class ExtendedTaskInfo
    {
        public int id { set; get; }
        public string title { set; get; }
        public ShortenLessonInfo lesson { set; get; }
        public string description { set; get; }
        public string criteria { set; get; }
        public bool isTeamWork { set; get; }
        public string link { set; get; }
        public string taskType { set; get; }
        public string deadline { set; get; }
    }

    public class LessonSuperExtendedInfo
    {
        public int id { set; get; }
        public int number { set; get; }
        public string title { set; get; }
        public string description { set; get; }
        public string date { set; get; }
        public string presLink { set; get; }
        public string videoLink { set; get; }

        public ExtendedTaskInfo homework { set; get; }
        public ExtendedTaskInfo test { set; get; }
    }

    public class LessonExtendedInfo
    {
        public int id { set; get; }
        public int number { set; get; }
        public string title { set; get; }
        public string description { set; get; }
        public string date { set; get; }
        public string presLink { set; get; }
        public string videoLink { set; get; }

        public TaskInLesson homework { set; get; }
        public TaskInLesson test { set; get; }

    }

    public class LessonResponse
    {
        public Pagination? pagination { set; get; }
        public List<LessonInfo> content { set; get; }
    }
}
