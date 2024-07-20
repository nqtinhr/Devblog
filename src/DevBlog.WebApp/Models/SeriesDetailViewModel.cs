using DevBlog.Core.Models;
using DevBlog.Core.Models.Content;

namespace DevBlog.WebApp.Models
{
    public class SeriesDetailViewModel
    {
        public SeriesDto Series { get; set; }

        public PagedResult<PostInListDto> Posts { get; set; }
    }
}
