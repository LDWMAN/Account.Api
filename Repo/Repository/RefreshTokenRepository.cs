using Microsoft.EntityFrameworkCore;
using AccountApi.Data;
using AccountApi.Model.Entity;
using AccountApi.Repo.IRepository;

namespace AccountApi.Repo.Repository
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(
            AppDbContext context,
            ILogger logger
            ) : base(context, logger)
        {

        }
        public async Task<RefreshToken> GetByRefreshToken(string refreshToken)
        {
            try
            {
                return await dbSet.Where(x => x.Token.Equals(refreshToken))
                .AsNoTracking()
                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByRefreshToken 메서드에서 에러가 발생하였습니다.", typeof(RefreshTokenRepository));
                return null;
            }
        }

        public async Task<bool> MakeRefreshTokenAsUsed(RefreshToken refreshToken)
        {
            try
            {
                var token = await dbSet.Where(x => x.Token.Equals(refreshToken.Token))
                .FirstOrDefaultAsync();
                if (token == null)
                {
                    return false;
                }
                token.IsUsed = true;

                dbSet.Update(token);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} MakeRefreshTokenAsUsed 메서드에서 에러가 발생하였습니다.", typeof(RefreshTokenRepository));
                return false;
            }
        }

        // public async Task<IEnumerable<RefreshToken>> GetRefreshTokenbyUsed(string userId)
        // {
        //     try
        //     {
        //         return await dbSet.Where(x => x.UserId.Equals(userId) && x.IsUsed == true)
        //         .AsNoTracking()
        //         .ToListAsync();
        //     }
        //     catch(Exception ex)
        //     {
        //         _logger.LogError(ex, "{Repo} GetRefreshTokenbyUserId 메서드에서 에러가 발생하였습니다.", typeof(UserRepository));
        //         return null;
        //     }
        // }

        public async Task<IEnumerable<RefreshToken>> GetRefreshTokenbyUserId(string userId)
        {
            try
            {
                return await dbSet.Where(x => x.UserId.Equals(userId))
                .AsNoTracking()
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetRefreshTokenbyUserId 메서드에서 에러가 발생하였습니다.", typeof(UserRepository));
                return null;
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetRefreshTokensAsUsed(string userId)
        {
            try
            {
                return await dbSet.Where(x => x.UserId.Equals(userId) && x.IsUsed == true)
                .AsNoTracking()
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetRefreshTokensAsUsed 메서드에서 에러가 발생하였습니다.", typeof(UserRepository));
                return null;
            }
        }

        public async Task RemoveRefreshToken(string tokenId)
        {
            try
            {
                var token = await dbSet.Where(x => x.RefreshTokenId.ToString().Equals(tokenId))
                //.AsNoTracking()
                .FirstOrDefaultAsync();

                dbSet.Remove(token);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} RemoveRefreshToken 메서드에서 에러가 발생하였습니다.", typeof(UserRepository));
            }
        }
    }
}