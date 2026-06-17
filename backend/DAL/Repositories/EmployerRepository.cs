using DAL.DTO.EmployerDtos;
using DAL.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class EmployerRepository : IEmployerRepository
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Employer> _passwordHasher;

        public EmployerRepository(AppDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Employer>();
        }

        public async Task<int> GetNewCountLastWeekAsync()
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            return await _context.Employers
                .CountAsync(e => e.CreatedAt >= oneWeekAgo);
        }

        public async Task<List<DalEmployerFullData>> GetAllAsync()
        {
            var employers = await _context.Employers.ToListAsync();
            return employers.Select(EmployerMapper.ToDal).ToList();
        }

        public async Task<DalEmployerFullData?> GetByIdAsync(int id)
        {
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Id == id);
            return employer == null ? null : EmployerMapper.ToDal(employer);
        }

        public async Task<DalEmployerFullData?> GetByEmailAsync(string email)
        {
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Email == email);
            return employer == null ? null : EmployerMapper.ToDal(employer);
        }

        public async Task<Employer?> GetEmployerEntityByEmailAsync(string email)
        {
            return await _context.Employers.FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<Employer> CreateAsync(DalEmployerCreate dto)
        {
            var employer = EmployerMapper.ToDomain(dto);
            employer.Password = _passwordHasher.HashPassword(null!, dto.Password);

            _context.Employers.Add(employer);
            await _context.SaveChangesAsync();
            return employer;
        }

        public async Task UpdateAsync(Employer employer)
        {
            _context.Employers.Update(employer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateEmployerInfoAsync(DalEmployerUpdate dto)
        {
            if (!int.TryParse(dto.Id, out var id))
                throw new ArgumentException("Invalid employer ID");

            var employer = await _context.Employers.FindAsync(id);
            if (employer == null) return false;

            employer.FirstName = dto.FirstName;
            employer.LastName = dto.LastName;
            employer.Email = dto.Email;
            employer.Phone = dto.Phone;
            employer.UpdatedAt = DateTime.UtcNow;

            _context.Employers.Update(employer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateEmployerPasswordAsync(int employerId, string newPassword)
        {
            var employer = await _context.Employers.FindAsync(employerId);
            if (employer == null) return false;

            employer.Password = _passwordHasher.HashPassword(employer, newPassword);
            employer.UpdatedAt = DateTime.UtcNow;

            _context.Employers.Update(employer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var employer = await _context.Employers.FindAsync(id);
            if (employer != null)
            {
                _context.Employers.Remove(employer);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Employer?> CheckPasswordAsync(string email, string password)
        {
            var employer = await _context.Employers.FirstOrDefaultAsync(e => e.Email == email);
            if (employer == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(employer, employer.Password, password);
            if (result != PasswordVerificationResult.Success) return null;

            return employer;
        }

        public async Task<Employer?> GetEmployerEntityByIdAsync(int id)
        {
            return await _context.Employers.FindAsync(id);
        }

        public async Task<bool> VerifyPasswordByIdAsync(int employerId, string password)
        {
            var employer = await _context.Employers.FindAsync(employerId);
            if (employer?.Password == null) return false;
            var result = _passwordHasher.VerifyHashedPassword(employer, employer.Password, password);
            return result == PasswordVerificationResult.Success;
        }

    }
}
