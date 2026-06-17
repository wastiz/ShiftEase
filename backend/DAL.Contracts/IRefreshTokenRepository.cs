using Domain;

namespace DAL.Contracts;

using Domain.Models;

public interface IRefreshTokenRepository
{
    Task SaveAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task InvalidateAsync(RefreshToken token);
    Task<int> DeleteExpiredAndInvalidAsync();
    Task DeleteByUserIdAsync(string userId);
}
