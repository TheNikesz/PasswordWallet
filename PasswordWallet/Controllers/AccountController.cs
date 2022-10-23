using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordWallet.DTOs.Account;
using PasswordWallet.Services;

namespace PasswordWallet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IUserAccessor _userAccessor;

    public AccountController(IAccountService accountService, IUserAccessor userAccessor)
    {
        _accountService = accountService;
        _userAccessor = userAccessor;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AccountDto>> Login(LoginDto loginDto)
    {
        if (!await _accountService.IfAccountExists(loginDto.Login)) return Unauthorized();

        if (!await _accountService.CheckPasswordWithLogin(loginDto.Login, loginDto.Password)) return Unauthorized();

        return await _accountService.Login(loginDto);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AccountDto>> Register(RegisterDto registerDto)
    {
        if (await _accountService.IfAccountExists(registerDto.Login)) return BadRequest("Login already used");

        return await _accountService.AddAccount(registerDto);
    }

    [Authorize]
    [HttpPatch("change-password")]
    public async Task<ActionResult<AccountDto>> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var userId = _userAccessor.GetUserId();
        if (userId == null) throw new KeyNotFoundException("User not logged in");
        if (!await _accountService.CheckPasswordWithId(Guid.Parse(userId), changePasswordDto.OldPassword))
            return Unauthorized();
        return await _accountService.ChangePassword(Guid.Parse(userId), changePasswordDto.NewPassword,
            changePasswordDto.IsPasswordKeptAsHash);
    }
}