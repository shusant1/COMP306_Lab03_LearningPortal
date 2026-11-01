
using System.ComponentModel.DataAnnotations;

namespace COMP306_Lab03_LearningPortal.Models
{
    public class ResourceCreateViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")]
        public int CategoryID { get; set; }
    }
}
