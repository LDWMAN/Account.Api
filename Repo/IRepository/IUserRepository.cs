using AccountApi.Model.Entity;

namespace AccountApi.Repo.IRepository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        //Task<bool> ExistEmailCheck(string email);

        Task<User> GetByEmail(string email);
    }
}