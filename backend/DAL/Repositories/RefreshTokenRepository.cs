using DAL.Contracts;
using Domain;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task InvalidateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteByUserIdAsync(string userId)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM \"RefreshTokens\" WHERE \"UserId\" = {userId}");
    }

    public async Task<int> DeleteExpiredAndInvalidAsync()
    {
        return await _context.Database.ExecuteSqlInterpolatedAsync($"""
                                                                        DELETE FROM "RefreshTokens"
                                                                        WHERE "Expires" < {DateTime.UtcNow}
                                                                           OR "IsUsed" = true
                                                                           OR "IsRevoked" = true
                                                                    """);
    }


    
    public async Task RemoveExpiredAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.Expires < DateTime.UtcNow || rt.IsRevoked || rt.IsUsed)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task UpdateTokenAsync(RefreshToken oldToken, RefreshToken newToken)
    {
        oldToken.IsUsed = true;
        oldToken.IsRevoked = true;

        _context.RefreshTokens.Update(oldToken);
        _context.RefreshTokens.Add(newToken);

        await _context.SaveChangesAsync();
    }


}
