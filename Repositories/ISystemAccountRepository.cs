using BusinessObjects;

namespace Repositories;

public interface ISystemAccountRepository
{
    Task<SystemAccount> Login(string email, string password);
}
