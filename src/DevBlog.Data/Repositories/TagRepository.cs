using AutoMapper;
using DevBlog.Core.Domain.Content;
using DevBlog.Core.Models.Content;
using DevBlog.Core.Repositories;
using DevBlog.Data.SeedWorks;
using Microsoft.EntityFrameworkCore;

namespace DevBlog.Data.Repositories
{
    public class TagRepository : RepositoryBase<Tag, Guid>, ITagRepository
    {
        private readonly IMapper _mapper;
        public TagRepository(DevBlogContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public async Task<TagDto?> GetBySlug(string slug)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Slug == slug);
            if (tag == null) return null;
            return _mapper.Map<TagDto?>(tag);
        }
    }
}
