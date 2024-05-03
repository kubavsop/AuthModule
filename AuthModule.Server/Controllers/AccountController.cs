using System.Security.Cryptography;
using AuthModule.Server.Data;
using AuthModule.Server.Data.Entities;
using AuthModule.Shared;
using AuthModule.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterModel registerModel)
    {
        if (await _context.Users.AnyAsync(u => u.Login == registerModel.Login))
        {
            return BadRequest();
        }

        await _context.Users.AddAsync(new User
        {
            Login = registerModel.Login,
            PublicKey = registerModel.PublicKey
        });
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Route("message")]
    public async Task<ActionResult<EncryptedMessageModel>> CreateMessage(
        CreateEncryptedMessageModel createEncryptedMessageModel)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == createEncryptedMessageModel.Login);

        if (user == null)
        {
            return NotFound();
        }

        var message = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        await _context.AddAsync(new EncryptedMessage
        {
            Message = Algorithm.SHA256Hash(message),
            MessageExpirationTime = DateTime.UtcNow.AddMinutes(15),
            User = user
        });

        await _context.SaveChangesAsync();

        return Ok(new EncryptedMessageModel
        {
            EncryptedMessage = Algorithm.RSAEncrypt(
                message,
                user.PublicKey,
                false
            )
        });
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginModel loginModel)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == loginModel.Login);
        if (user == null) return BadRequest();

        var hashedMessage = Algorithm.SHA256Hash(loginModel.DecryptedMessage);

        var message = await _context.Messages.FirstOrDefaultAsync(m => m.UserId == user.Id);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return Ok();
        }

        return BadRequest();
    }
}