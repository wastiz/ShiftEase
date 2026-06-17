using Domain;

namespace DAL.Contracts;

public interface IPasswordResetRepository
{
    Task<PasswordResetToken> CreateTokenAsync(string email, string token, DateTime expiresAt);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task MarkAsUsedAsync(string token);
    Task DeleteExpiredTokensAsync();
    Task DeleteByEmailAsync(string email);
}
