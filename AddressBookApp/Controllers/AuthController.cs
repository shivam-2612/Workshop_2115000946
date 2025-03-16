using Business_Layer.Interface;
using Business_Layer.Service;
using Microsoft.AspNetCore.Mvc;
using Model_Layer.DTO;
using Repository_Layer.Service;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAddressBookBL _addressBookBL;
    private readonly RedisCacheService redisCache;
    private readonly RabbitMQService rabbitMQService;

    public AuthController(IAddressBookBL _addressBookBL, RedisCacheService redisCache, RabbitMQService rabbitMQService)
    {
        this._addressBookBL = _addressBookBL;
        this.redisCache = redisCache;
        this.rabbitMQService= rabbitMQService;
    }

    [HttpPost("register")]
    public IActionResult Register(UserDTO userDto)
    {
        var result = _addressBookBL.Register(userDto);
        if (result == "User already exists")
            return BadRequest(new { message = result });
        rabbitMQService.PublishMessage("user.registered", result);

        return Ok(new { message = result });
    }

    [HttpPost("login")]
    public IActionResult Login(UserDTO userDto)
    {
        var token = _addressBookBL.Login(userDto);
        if (token == null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(new { token });
    }

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
    {
        var result = _addressBookBL.ForgotPassword(request.Email);
        return result == "User not found" ? NotFound(new { message = result }) : Ok(new { message = result });
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
    {
        var result = _addressBookBL.ResetPassword(resetPasswordDto.Token, resetPasswordDto.NewPassword);
        return result == "Invalid or expired token" ? BadRequest(new { message = result }) : Ok(new { message = result });
    }
}
