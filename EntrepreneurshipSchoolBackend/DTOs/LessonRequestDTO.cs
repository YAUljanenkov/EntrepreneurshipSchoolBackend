using System.Globalization;

namespace EntrepreneurshipSchoolBackend.DTOs
{
    public class LessonRequest
    {
        public int? id { set; get; }
        public int number { set; get; }
        public string title { set; get; }
        public string description { set; get; }
        public DateTime date { set; get; }
        public string? presLink { set; get; }
        public string? videoLink { set; get; }
        public int? homeworkId { set; get; }
        public int? testId { set; get; }
    }

    public class LessonComplexRequest
    {
        public int? lessonNumber { set; get; }

        public string? lessonTitle { set; get; }
        public string? sortProperty { set; get; }
        public string? sortOrder { set; get; }
        public bool pageable { set; get; }
        public int? page { set; get; } = 1;
        public int? pageSize { set; get; } = 10;
    }

}
