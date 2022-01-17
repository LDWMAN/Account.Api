using AccountApi.Data;
using AccountApi.Repo.IRepository;
using AccountApi.Repo.Repository;

namespace AccountApi.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext _context;
    private readonly ILogger _logger;

    ///Add Repository Instance
    public IUserRepository User { get; private set; }
    public IRefreshTokenRepository RefreshToken { get; private set; }

    public UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = loggerFactory.CreateLogger("dblogs");

        ///Repository Mapping
        User = new UserRepository(context, _logger);
        RefreshToken = new RefreshTokenRepository(context, _logger);
    }

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
