using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Solutions")]
    [PrimaryKey(nameof(TaskId), nameof(LearnerId), nameof(GroupId))]
    public class Solution
    {
        public int TaskId { get; set; }
        [Required] public Task? Task { get; set; }
        public int LearnerId { get; set; }
        public Learner? Learner { get; set; }
        public int GroupId { get; set; }
        public Group? Group { get; set; }

        [Required] public DateTime CompleteDate { get; set; }
        public int fileId { get; set; }
        public UserFile file { get; set; } 
    }
}
