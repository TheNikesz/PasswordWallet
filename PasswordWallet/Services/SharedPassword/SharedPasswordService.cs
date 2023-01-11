using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using PasswordWallet.DTOs.SharedPassword;
using PasswordWallet.Exceptions;

namespace PasswordWallet.Services.SharedPassword;

public class SharedPasswordService : ISharedPasswordService
{
    private readonly DataBaseContext _dataContext;
    private readonly IUserAccessor _userAccessor;
    private readonly ICryptographicService _cryptoService;
    private readonly IHashingService _hashService;
    private readonly IMapper _mapper;

    public SharedPasswordService(DataBaseContext dataContext, IUserAccessor userAccessor,
        ICryptographicService cryptoService, IHashingService hashService,
        IMapper mapper)
    {
        _dataContext = dataContext;
        _userAccessor = userAccessor;
        _cryptoService = cryptoService;
        _hashService = hashService;
        _mapper = mapper;
    }

    public async Task<SharedPasswordDto> CreateSharedPassword(SharedPasswordMiniDto sharedPasswordMiniDto)
    {
        // Get Account
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) throw new Exception();
        var owner =
            await _dataContext.Accounts
                .FirstOrDefaultAsync(x => x.Id == Guid.Parse(accountId));
        if (owner == null) throw new Exception();

        // Check if user shares to himself
        if (owner.Login == sharedPasswordMiniDto.Login) throw new SelfShareException();

        // Get SavedPassword
        var savedPassword =
            await _dataContext.SavedPasswords.Include(x => x.SharedPasswords)
                .FirstOrDefaultAsync(x => x.Id == sharedPasswordMiniDto.Id);
        if (savedPassword == null) throw new Exception();

        var toAccount =
            await _dataContext.Accounts.FirstOrDefaultAsync(x => x.Login == sharedPasswordMiniDto.Login);
        if (toAccount == null) throw new Exception();


        // Check if already shared
        if (savedPassword.SharedPasswords.Any(x =>
                x.AccountId == toAccount.Id))
            throw new AlreadySharedException();

        var sharedPassword = new Entities.SharedPassword
        {
            AccountId = toAccount.Id,
            SavedPasswordId = savedPassword.Id,
        };
        // Save to Database
        await _dataContext.SharedPasswords.AddAsync(sharedPassword);
        await _dataContext.SaveChangesAsync();
        return new SharedPasswordDto
        {
            Id = sharedPassword.Id,
            SavedPasswordId = sharedPassword.SavedPasswordId,
            Description = savedPassword.Description,
            Login = savedPassword.Login,
            Owner = savedPassword.Account.Login,
            WebAddress = savedPassword.WebAddress,
        };
    }

    public async Task<string> DecryptSharedPassword(Guid id)
    {
        // Get SharedPassword
        var sharedPassword =
            await _dataContext.SharedPasswords.Include(x => x.SavedPassword)
                .FirstOrDefaultAsync(x => x.Id == id);
        if (sharedPassword == null) throw new Exception();

        //Get Owner Account
        var owner = await _dataContext.Accounts.FirstOrDefaultAsync(x =>
            x.Id == sharedPassword.SavedPassword.AccountId);
        if (owner == null) throw new Exception();

        var masterPasswordBytes = GetMasterPasswordBytes(owner.PasswordHash);

        var ivBytes = Convert.FromBase64String(sharedPassword.SavedPassword.Iv);
        return _cryptoService.Decrypt(sharedPassword.SavedPassword.Password, masterPasswordBytes, ivBytes);
    }

    public async Task DeleteSharedPassword(Guid id)
    {
        // Get SavedPassword
        var sharedPassword =
            await _dataContext.SharedPasswords.FirstOrDefaultAsync(x => x.Id == id);
        if (sharedPassword == null) throw new Exception();

        _dataContext.SharedPasswords.Remove(sharedPassword);
        var result = await _dataContext.SaveChangesAsync();
        if (result <= 0) throw new Exception();
    }

    public async Task<List<SharedPasswordDto>> ListSharedPassword()
    {
        // Get AccountGuid
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) throw new Exception();
        var accountGuid = Guid.Parse(accountId);

        return await _dataContext.SharedPasswords
            .Where(x => x.AccountId == accountGuid)
            .ProjectTo<SharedPasswordDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<bool> IsOwner(Guid id, Guid accountId)
    {
        var sharedPassword = await _dataContext.SharedPasswords.Include(x => x.SavedPassword)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (sharedPassword == null) throw new Exception();

        return sharedPassword.SavedPassword.AccountId == accountId;
    }

    private byte[] GetMasterPasswordBytes(string password)
    {
        return Convert.FromBase64String(
            _hashService.HashWithMD5(_hashService.HashWithHMAC(password, "16characterslong")));
    }
}