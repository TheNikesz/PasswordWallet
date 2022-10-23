using PasswordWallet.DTOs.SavedPassword;

namespace PasswordWallet.Services;

public interface ISavedPasswordService
{
    public Task<SavedPasswordDto?> DetailPassword(Guid id);
    public Task<List<SavedPasswordDto>> ListPassword();
    public Task<SavedPasswordDto> CreatePassword(AddPasswordDto passwordDto);
    public Task DeletePassword(Guid id);
    public Task<string> DecryptPassword(Guid id);
}