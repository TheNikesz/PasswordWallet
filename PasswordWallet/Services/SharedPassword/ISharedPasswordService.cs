using PasswordWallet.DTOs.SharedPassword;

namespace PasswordWallet.Services.SharedPassword;

public interface ISharedPasswordService
{
    public Task<SharedPasswordDto> CreateSharedPassword(SharedPasswordMiniDto sharedPasswordMiniDto);
    public Task<string> DecryptSharedPassword(Guid id);
    public Task DeleteSharedPassword(Guid id);
    public Task<List<SharedPasswordDto>> ListSharedPassword();
    public Task<bool> IsOwner(Guid id, Guid accountId);
}