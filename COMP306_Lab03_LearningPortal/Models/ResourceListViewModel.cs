
namespace COMP306_Lab03_LearningPortal.Models
{
    public class ResourceListViewModel
    {
        public int ResourceID { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public int ViewCount { get; set; }
        public int AuthorID { get; set; }

    }
}
