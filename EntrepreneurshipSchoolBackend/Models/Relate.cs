using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntrepreneurshipSchoolBackend.Models
{
    [Table("Relates")]
    [PrimaryKey(nameof(LearnerId), nameof(GroupId))]
    public class Relate
    {
        public int LearnerId { get; set; }
        public int GroupId { get; set; }
        public virtual Learner Learner { get; set; }
        public virtual Group Group { get; set; }

    }
}
