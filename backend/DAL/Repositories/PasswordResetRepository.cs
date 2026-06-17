using DAL.Contracts;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly AppDbContext _context;

    public PasswordResetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken> CreateTokenAsync(string email, string token, DateTime expiresAt)
    {
        var resetToken = new PasswordResetToken
        {
            Email = email,
            Token = token,
            ExpiresAt = expiresAt,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        return resetToken;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task MarkAsUsedAsync(string token)
    {
        var resetToken = await GetByTokenAsync(token);
        if (resetToken != null)
        {
            resetToken.IsUsed = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
            .ToListAsync();

        _context.PasswordResetTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteByEmailAsync(string email)
    {
        var tokens = await _context.PasswordResetTokens
            .Where(t => t.Email == email)
            .ToListAsync();
        _context.PasswordResetTokens.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }
}
