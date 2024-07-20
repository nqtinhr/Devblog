using AutoMapper;
using DevBlog.Core.Domain.Identity;
using DevBlog.Core.Repositories;
using DevBlog.Core.SeedWorks;
using DevBlog.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace DevBlog.Data.SeedWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DevBlogContext _context;

        public UnitOfWork(DevBlogContext context, IMapper mapper, UserManager<AppUser> userManager)
        {
            _context = context;
            Posts = new PostRepository(context, mapper, userManager);
            PostCategories = new PostCategoryRepository(context, mapper);
            Series = new SeriesRepository(context, mapper);
            Transactions = new TransactionRepository(context, mapper);
            Users = new UserRepository(context);
            Tags = new TagRepository(context, mapper);
        }
        public IPostRepository Posts { get; private set; }
        public IPostCategoryRepository PostCategories { get; private set; }
        public ISeriesRepository Series { get; private set; }
        public ITransactionRepository Transactions { get; private set; }

        public IUserRepository Users { get; private set; }
        public ITagRepository Tags { get; private set; }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
