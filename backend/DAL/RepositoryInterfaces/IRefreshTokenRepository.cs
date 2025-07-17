using Domain;

namespace DAL.RepositoryInterfaces;

using Domain.Models;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task InvalidateAsync(RefreshToken token);
}
