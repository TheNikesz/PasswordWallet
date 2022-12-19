using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Application.Accounts.DAOs;
using PasswordWallet.DTOs.Account;
using PasswordWallet.Services;

namespace PasswordWallet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IUserAccessor _userAccessor;
    private readonly ILoginAttemptsService _loginAttemptsService;

    public AccountController(IAccountService accountService, IUserAccessor userAccessor,
        ILoginAttemptsService loginAttemptsService)
    {
        _accountService = accountService;
        _userAccessor = userAccessor;
        _loginAttemptsService = loginAttemptsService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AccountDto>> Login(LoginDto loginDto)
    {
        var account = await _accountService.GetAccount(loginDto.Login);
        if (account == null) return Unauthorized();

        var ipAddress = _userAccessor.GetRequestIpAddress();

        var blockLogInTime = await _loginAttemptsService.ThrottleLogInTime(account.Id);
        var blockIpAddressTime = await _loginAttemptsService.ThrottleIpLogIn(account.Id, ipAddress);
        if (blockIpAddressTime == int.MaxValue)
        {
            await _loginAttemptsService.LogLoginAttempt(account.Id, false, ipAddress);
            return Unauthorized("Your IP Address is permanently blocked");
        }

        var blockTime = blockIpAddressTime > blockLogInTime ? blockIpAddressTime : blockLogInTime;
        if (blockTime > 0)
        {
            var lastUnsuccessfulDateTime =
                await _loginAttemptsService.LastUnsuccessfulLoginAttemptTime(account.Id) ?? DateTime.Now;
            var blockTimeDelta = DateTime.Now.Subtract(lastUnsuccessfulDateTime).Seconds;
            if (blockTimeDelta < blockTime)
                return Unauthorized(
                    $"Too many unsuccessful attempts - account will stay locked for {blockTime - blockTimeDelta}s");
        }

        if (!await _accountService.CheckPasswordWithLogin(loginDto.Login, loginDto.Password))
        {
            await _loginAttemptsService.LogLoginAttempt(account.Id, false, ipAddress);
            return Unauthorized("Unsuccessful login");
        }

        await _loginAttemptsService.LogLoginAttempt(account.Id, true, ipAddress);
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

    [Authorize]
    [HttpGet("login-statistics")]
    public async Task<ActionResult<string>> LoginStatistics()
    {
        var userId = _userAccessor.GetUserId();
        if (userId == null) return Unauthorized();

        var accountId = Guid.Parse(userId);

        var successfulLoginTime = await _loginAttemptsService.LastSuccessfulLoginAttemptTime(accountId);
        var unsuccessfulLoginTime = await _loginAttemptsService.LastUnsuccessfulLoginAttemptTime(accountId);

        var message = "";
        message += successfulLoginTime == null
            ? "No successful logins"
            : $"Last successful login at: {successfulLoginTime}";
        message += unsuccessfulLoginTime == null
            ? "No unsuccessful logins"
            : $"Last unsuccessful login at: {unsuccessfulLoginTime}";
        return message;
    }

    [Authorize]
    [HttpGet("ipaddress-lock")]
    public async Task<ActionResult<List<IpAddressBlockDao>>> ListAddressBlocks()
    {
        var userId = _userAccessor.GetUserId();
        if (userId == null) return Unauthorized();
        var accountId = Guid.Parse(userId);

        return await _loginAttemptsService.ListIpAddressBlocks(accountId);
    }

    [Authorize]
    [HttpDelete("ipaddress-unlock")]
    public async Task<ActionResult> UnlockIpAddress([FromBody] Ip ipAddress)
    {
        var userId = _userAccessor.GetUserId();
        if (userId == null) return Unauthorized();

        var accountId = Guid.Parse(userId);

        await _loginAttemptsService.UnlockIpAddress(accountId, ipAddress.ipAddress);
        return Ok();
    }

    public class Ip
    {
        public string ipAddress { get; set; }
    }
}