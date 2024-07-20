using DevBlog.Core.Models;
using DevBlog.Core.Models.Content;

namespace DevBlog.WebApp.Models
{
    public class PostListByCategoryViewModel
    {
        public PostCategoryDto Category { get; set; }
        public PagedResult<PostInListDto> Posts { get; set; }
    }
}
