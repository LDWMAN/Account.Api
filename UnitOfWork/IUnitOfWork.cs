
using AccountApi.Repo.IRepository;

namespace AccountApi.UnitOfWork;

public interface IUnitOfWork
{
    IUserRepository User {get;}
    IRefreshTokenRepository RefreshToken { get; }
    Task CompleteAsync();
}
