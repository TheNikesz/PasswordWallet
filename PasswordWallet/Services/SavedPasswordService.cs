using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PasswordWallet.DTOs.SavedPassword;
using PasswordWallet.Entities;
using Aes = System.Security.Cryptography.Aes;

namespace PasswordWallet.Services;

public class SavedPasswordService : ISavedPasswordService
{
    private readonly DataBaseContext _dataContext;
    private readonly ICryptographicService _cryptoService;
    private readonly IHashingService _hashService;
    private readonly IUserAccessor _userAccessor;
    private readonly IMapper _mapper;

    public SavedPasswordService(DataBaseContext dataContext, ICryptographicService cryptoService, IHashingService hashService,
        IUserAccessor userAccessor, IMapper mapper)
    {
        _dataContext = dataContext;
        _cryptoService = cryptoService;
        _hashService = hashService;
        _userAccessor = userAccessor;
        _mapper = mapper;
    }

    public async Task<List<SavedPasswordDto>> ListPassword()
    {
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) throw new Exception();
        return await _dataContext.SavedPasswords.Where(x => x.AccountId == Guid.Parse(accountId))
            .ProjectTo<SavedPasswordDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<SavedPasswordDto?> DetailPassword(Guid id)
    {
        return await _dataContext.SavedPasswords.ProjectTo<SavedPasswordDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<string> DecryptPassword(Guid id)
    {
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) throw new Exception();
        var account = await _dataContext.Accounts.FirstOrDefaultAsync(x => x.Id == Guid.Parse(accountId));
        if (account == null) throw new Exception();

        var savedPassword = await _dataContext.SavedPasswords.FirstOrDefaultAsync(x => x.Id == id);
        if (savedPassword == null) throw new Exception();
        var masterPasswordBytes = GetMasterPasswordBytes(account.PasswordHash);

        var ivBytes = Convert.FromBase64String(savedPassword.Iv);
        return _cryptoService.Decrypt(savedPassword.Password, masterPasswordBytes, ivBytes);
    }

    public async Task<SavedPasswordDto> CreatePassword(AddPasswordDto passwordDto)
    {
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) throw new Exception();
        var account = await _dataContext.Accounts.FirstOrDefaultAsync(x => x.Id == Guid.Parse(accountId));
        if (account == null) throw new Exception();

        var masterPasswordBytes = GetMasterPasswordBytes(account.PasswordHash);
        using var aes = Aes.Create();
        var ivBytes = aes.IV;
        var savedPassword = new SavedPassword
        {
            Password = _cryptoService.Encrypt(passwordDto.Password, masterPasswordBytes, ivBytes),
            WebAddress = passwordDto.WebAddress,
            Description = passwordDto.Description,
            Login = passwordDto.Login,
            Iv = Convert.ToBase64String(ivBytes),
            AccountId = Guid.Parse(accountId),
        };

        await _dataContext.SavedPasswords.AddAsync(savedPassword);
        var result = await _dataContext.SaveChangesAsync();
        if (result <= 0) throw new Exception("Error saving new Password to Database");
        return _mapper.Map<SavedPassword, SavedPasswordDto>(savedPassword);
    }
    

    public async Task DeletePassword(Guid id)
    {
        var savedPassword = await _dataContext.SavedPasswords.FirstOrDefaultAsync(x => x.Id == id);
        if (savedPassword == null) throw new Exception();

        _dataContext.SavedPasswords.Remove(savedPassword);
        var result = await _dataContext.SaveChangesAsync();
        if (result <= 0) throw new Exception();
    }

    private byte[] GetMasterPasswordBytes(string password)
    {
        return Convert.FromBase64String(
            _hashService.HashWithMD5(_hashService.HashWithHMAC(password, "16characterslong")));
    }
}