using System.Security.Cryptography;
using AuthModule.Client.Entities;
using AuthModule.Shared;
using AuthModule.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AuthModule.Client.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController: ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(ClientAuthModel authModel)
    {
        if (await _context.Users.AnyAsync(u => u.Login == authModel.Login))
            return BadRequest();
        using var rsa = new RSACryptoServiceProvider();

        rsa.KeySize = 1024;
        var publicKey = rsa.ExportRSAPublicKey();
        var privateKey = rsa.ExportRSAPrivateKey();
        var publicKeyString = Convert.ToBase64String(publicKey);
        var privateKeyString = Convert.ToBase64String(privateKey);

        await _context.Users.AddAsync(new User
        {
            Login = authModel.Login,
            PublicKey = publicKeyString,
            EncryptedPrivateKey = Algorithm.AesEncrypt(privateKeyString, authModel.Passphrase)
        });
        
        var httpRequester = new HttpRequester();
        
        var registerModel = new RegisterModel
        {
            Login = authModel.Login,
            PublicKey = publicKeyString
        };
        var registerResponse = await httpRequester.RegisterAsync(registerModel);
        if (!registerResponse.IsSuccessStatusCode) return BadRequest();
        
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(ClientAuthModel authModel)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == authModel.Login);
        if (user == null) return NotFound();

        var success = Algorithm.AesDecrypt(user.EncryptedPrivateKey, authModel.Passphrase, out var decryptedPrivateKey);
        if (!success) return BadRequest("Ошибка расшифровки ключа");
        
        var httpRequester = new HttpRequester();

        var createMessageModel = new CreateEncryptedMessageModel
        {
            Login = user.Login,
            PublicKey = user.PublicKey
        };

        var messageModel = await httpRequester.GetMessageAsync(createMessageModel);
        
        var decryptedMessage = Algorithm.RSADecrypt(messageModel.EncryptedMessage, decryptedPrivateKey, false);
        var loginModel = new LoginModel
        {
            Login = user.Login,
            DecryptedMessage = decryptedMessage
        };

        var loginResponse = await httpRequester.LoginAsync(loginModel);
        if (!loginResponse.IsSuccessStatusCode) return BadRequest();

        return Ok();
    }
}