namespace AuthModule.Shared.Models;

public sealed class LoginModel
{
    public required string DecryptedMessage { get; set; }
    public required string Login { get; set; }
}