using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordWallet.DTOs.SharedPassword;
using PasswordWallet.Exceptions;
using PasswordWallet.Services;
using PasswordWallet.Services.SharedPassword;

namespace PasswordWallet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SharedPasswordsController : ControllerBase
{
    private readonly IUserAccessor _userAccessor;
    private readonly ISavedPasswordService _savedPasswordService;
    private readonly ISharedPasswordService _sharedPasswordService;

    public SharedPasswordsController(IUserAccessor userAccessor, ISavedPasswordService savedPasswordService,
        ISharedPasswordService sharedPasswordService)
    {
        _userAccessor = userAccessor;
        _savedPasswordService = savedPasswordService;
        _sharedPasswordService = sharedPasswordService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<SharedPasswordDto>>> ListSharedPassword()
    {
        return await _sharedPasswordService.ListSharedPassword();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SharedPasswordDto>> CreateSharedPassword(SharedPasswordMiniDto sharedPasswordMiniDto)
    {
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) return Unauthorized();
        var accountGuid = Guid.Parse(accountId);

        if (!await _savedPasswordService.IsOwner(sharedPasswordMiniDto.Id, accountGuid))
            return Forbid();
        
        try
        {
            return await _sharedPasswordService.CreateSharedPassword(sharedPasswordMiniDto);
        }
        catch (SelfShareException e)
        {
            return BadRequest("Self-share attempt");
        }
        catch (AlreadySharedException e)
        {
            return BadRequest("Password already shared with this user");
        }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteSharedPassword(Guid id)
    {
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) return Unauthorized();
        var accountGuid = Guid.Parse(accountId);

        if (!await _sharedPasswordService.IsOwner(id, accountGuid))
            return Forbid();

        await _sharedPasswordService.DeleteSharedPassword(id);
        return Ok();
    }

    [Authorize]
    [HttpGet("decrypt/{id:guid}")]
    public async Task<ActionResult<string>> DecryptSharedPassword(Guid id)
    {
        var accountId = _userAccessor.GetUserId();
        if (accountId == null) return Unauthorized();
        var accountGuid = Guid.Parse(accountId);

        if (!await _sharedPasswordService.IsOwner(id, accountGuid))
            return Forbid();

        return await _sharedPasswordService.DecryptSharedPassword(id);
    }
}