using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Application.Accounts.DAOs;
using PasswordWallet.Entities;

namespace PasswordWallet.Services;

public class LoginAttemptsService : ILoginAttemptsService
{
    private readonly DataBaseContext _context;

    public LoginAttemptsService(DataBaseContext context)
    {
        _context = context;
    }

    public async Task LogLoginAttempt(Guid accountId, bool isSuccessful, string ipAddress)
    {
        await _context.LoginAttempts.AddAsync(new LoginAttempt
        {
            AccountId = accountId,
            Time = DateTime.Now,
            IpAddress = ipAddress,
            IsSuccessful = isSuccessful,
        });
        await _context.SaveChangesAsync();
    }

    public async Task<DateTime?> LastSuccessfulLoginAttemptTime(Guid accountId)
    {
        return (await _context.LoginAttempts.Where(x => x.AccountId == accountId && x.IsSuccessful == true)
                .OrderByDescending(x => x.Time).FirstOrDefaultAsync())
            ?.Time;
    }

    public async Task<DateTime?> LastUnsuccessfulLoginAttemptTime(Guid accountId)
    {
        return (await _context.LoginAttempts.Where(x => x.AccountId == accountId && x.IsSuccessful == false)
                .OrderByDescending(x => x.Time).FirstOrDefaultAsync())
            ?.Time;
    }

    public async Task<int> ThrottleLogInTime(Guid accountId)
    {
        var attempts = await _context.LoginAttempts.Where(x => x.AccountId == accountId).OrderByDescending(x => x.Time)
            .ToListAsync();
        var unsuccessfulAttempts = 0;

        foreach (var attempt in attempts)
        {
            if (attempt.IsSuccessful == false) unsuccessfulAttempts++;
            else break;
        }

        switch (unsuccessfulAttempts)
        {
            case >= 4:
                return 120;
            case 3:
                return 10;
            case 2:
                return 5;
        }

        return 0;
    }

    public async Task<int> ThrottleIpLogIn(Guid accountId, string ipAddress)
    {
        var block = await _context.IpAddressBlocks.FirstOrDefaultAsync(x =>
            x.IpAddress == ipAddress && x.AccountId == accountId);
        if (block != null) return int.MaxValue;

        var attempts = await _context.LoginAttempts.Where(x => x.IpAddress == ipAddress).OrderByDescending(x => x.Time)
            .ToListAsync();
        var unsuccessfulAttempts = 0;

        foreach (var attempt in attempts)
        {
            if (attempt.IsSuccessful == false) unsuccessfulAttempts++;
            else break;
        }

        switch (unsuccessfulAttempts)
        {
            case >= 4:
                await _context.IpAddressBlocks.AddAsync(new IpAddressBlock
                    { IpAddress = ipAddress, AccountId = accountId });
                await _context.SaveChangesAsync();
                return int.MaxValue;
            case 3:
                return 10;
            case 2:
                return 5;
        }

        return 0;
    }

    public async Task<List<IpAddressBlockDao>> ListIpAddressBlocks(Guid accountId)
    {
        var ipBlocks = await _context.IpAddressBlocks
            .Where(x => x.AccountId == accountId).ToListAsync();
        var ipBlockDaos = new List<IpAddressBlockDao>();
        foreach (var ipBlock in ipBlocks)
        {
            ipBlockDaos.Add(new IpAddressBlockDao
            {
                AccountId = ipBlock.AccountId,
                IpAddress = ipBlock.IpAddress,
            });
        }
        return ipBlockDaos;
    }

    public async Task UnlockIpAddress(Guid accountId, string ipAddress)
    {
        var block = await _context.IpAddressBlocks.FirstOrDefaultAsync(x =>
            x.IpAddress == ipAddress && x.AccountId == accountId);
        if (block != null)
        {
            _context.IpAddressBlocks.Remove(block);
            await _context.SaveChangesAsync();
        }
    }
}