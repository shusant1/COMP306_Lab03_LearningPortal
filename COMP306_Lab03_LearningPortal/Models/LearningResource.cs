
namespace COMP306_Lab03_LearningPortal.Models
{
    public class LearningResource
    {
        public int ResourceID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int CategoryID { get; set; }
        public int AuthorID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? S3Key { get; set; }
        public int ViewCount { get; set; }
    }
}
