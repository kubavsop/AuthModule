using System.Security.Cryptography;
using AuthModule.Shared;
using AuthModule.Shared.Models;

namespace AuthModule.Client;

public class Test
{
    public async static Task Main()
    {
        const string login = "log916in605716756";
        using var rsa = new RSACryptoServiceProvider();

        rsa.KeySize = 1024;

        var publicKey = rsa.ExportRSAPublicKey();
        var privateKey = rsa.ExportRSAPrivateKey();
        var publicKeyString = Convert.ToBase64String(publicKey);
        var privateKeyString = Convert.ToBase64String(privateKey);


        var httpRequester = new HttpRequester();
        var registerModel = new RegisterModel
        {
            Login = login,
            PublicKey = publicKeyString
        };
        var registerResponse = await httpRequester.RegisterAsync(registerModel);

        registerResponse.EnsureSuccessStatusCode();

        var createMessageModel = new CreateEncryptedMessageModel
        {
            Login = login,
            PublicKey = publicKeyString
        };

        var messageModel = await httpRequester.GetMessageAsync(createMessageModel);

        var decryptedMessage = Algorithm.RSADecrypt(messageModel.EncryptedMessage, privateKeyString, false);
        var loginModel = new LoginModel
        {
            Login = login,
            DecryptedMessage = decryptedMessage
        };

        var loginResponse = await httpRequester.LoginAsync(loginModel);
        Console.WriteLine(loginResponse.StatusCode);
    }
}