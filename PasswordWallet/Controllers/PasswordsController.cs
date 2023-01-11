using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordWallet.DTOs.SavedPassword;
using PasswordWallet.Services;

namespace PasswordWallet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PasswordsController : ControllerBase
{
    private readonly ISavedPasswordService _savedPasswordService;
    private readonly IUserAccessor _userAccessor;

    public PasswordsController(ISavedPasswordService savedPasswordService, IUserAccessor userAccessor)
    {
        _savedPasswordService = savedPasswordService;
        _userAccessor = userAccessor;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SavedPasswordDto>>> ListPasswords()
    {
        return await _savedPasswordService.ListPassword();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SavedPasswordDto>> CreatePassword(AddPasswordDto passwordDto)
    {
        return await _savedPasswordService.CreatePassword(passwordDto);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeletePassword(Guid id)
    {
        var password = await _savedPasswordService.DetailPassword(id);
        var accountId = _userAccessor.GetUserId();
        if (password == null) return NotFound();
        if (accountId == null) throw new KeyNotFoundException("Account not found");
        if (password.AccountId != Guid.Parse(accountId)) return Forbid();

        await _savedPasswordService.DeletePassword(id);
        return Ok();
    }

    [Authorize]
    [HttpGet("decrypt/{id:guid}")]
    public async Task<ActionResult<string>> DecryptPassword(Guid id)
    {
        var password = await _savedPasswordService.DetailPassword(id);
        var accountId = _userAccessor.GetUserId();
        if (password == null) return NotFound();
        if (accountId == null) throw new KeyNotFoundException("Account not found");
        if (password.AccountId != Guid.Parse(accountId)) return Forbid();

        return await _savedPasswordService.DecryptPassword(id);
    }
}