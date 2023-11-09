using System.ComponentModel.DataAnnotations;
using App.Models.Blogs;

namespace App.Areas.Blogs.Models
{
    public class CreatePostModel : Post
    {
        [Required(ErrorMessage = "Phải có {0} bài viết")]
        [Display(Name = "Chuyên mục")]
        public int[] CategoryIDs { get; set; }
    }
}