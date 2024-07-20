using DevBlog.Core.Models;
using DevBlog.Core.Models.Content;

namespace DevBlog.WebApp.Models
{
    public class PostListByTagViewModel
    {
        public TagDto Tag { get; set; }
        public PagedResult<PostInListDto> Posts { get; set; }
    }
}
