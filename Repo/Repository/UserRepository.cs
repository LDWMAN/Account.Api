using System.Linq;
using AccountApi.Data;
using AccountApi.Model.Entity;
using AccountApi.Repo.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AccountApi.Repo.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context,
            ILogger logger) : base(context, logger)
        {

        }

        public async Task<User> GetByEmail(string email)
        {
            try
            {
                return await dbSet.Where(x => x.Email.Equals(email))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByEmail 메서드에서 에러가 발생하였습니다.", typeof(UserRepository));
                return null;
            }
        }
    }
}