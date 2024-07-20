using DevBlog.Core.Domain.Royalty;
using DevBlog.Core.Models;
using DevBlog.Core.Models.Royalty;
using DevBlog.Core.SeedWorks;

namespace DevBlog.Core.Repositories
{
    public interface ITransactionRepository : IRepository<Transaction, Guid>
    {
        Task<PagedResult<TransactionDto>> GetAllPaging(string? userName, int fromMonth, int fromYear, int toMonth, int toYear, int pageIndex = 1, int pageSize = 10);
    }
}
