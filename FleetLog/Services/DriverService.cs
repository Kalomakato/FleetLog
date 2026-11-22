using FleetLog.Data;
using FleetLog.Data.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FleetLog.Services;

public class DriverService
{
    private readonly FleetLogDbContext _context;

    public DriverService(FleetLogDbContext context)
    {
        _context = context;
    }

    public async Task<DriverCreateResult> CreateAsync(
        string fullName,
        string userId)
    {
        var name = fullName?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            return DriverCreateResult.InvalidName;
        }

        if (string.IsNullOrWhiteSpace(userId) ||
            !await _context.Users.AnyAsync(u => u.Id == userId))
        {
            return DriverCreateResult.AccountNotFound;
        }

        if (await _context.Drivers.AnyAsync(d => d.UserId == userId))
        {
            return DriverCreateResult.AccountAlreadyLinked;
        }

        var driver = new Driver
        {
            FullName = name,
            UserId = userId,
            IsActive = true
        };

        _context.Drivers.Add(driver);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException sqlException &&
                  (sqlException.Number == 2601 ||
                   sqlException.Number == 2627))
        {
            _context.Entry(driver).State = EntityState.Detached;
            return DriverCreateResult.AccountAlreadyLinked;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqlException sqlException &&
                  sqlException.Number == 547)
        {
            _context.Entry(driver).State = EntityState.Detached;
            return DriverCreateResult.AccountNotFound;
        }

        return DriverCreateResult.Success;
    }
}

public enum DriverCreateResult
{
    Success,
    InvalidName,
    AccountNotFound,
    AccountAlreadyLinked
}
