using AccountApi.Model.Entity;

namespace AccountApi.Repo.IRepository
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetByRefreshToken(string refreshToken);
        Task<bool> MakeRefreshTokenAsUsed(RefreshToken refreshToken);
        Task<IEnumerable<RefreshToken>> GetRefreshTokenbyUserId(string userId);
        Task<IEnumerable<RefreshToken>> GetRefreshTokensAsUsed(string userId);
        Task RemoveRefreshToken(string tokenId);
    }
}