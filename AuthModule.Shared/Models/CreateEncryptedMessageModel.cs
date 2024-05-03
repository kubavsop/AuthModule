namespace AuthModule.Shared.Models;

public sealed class CreateEncryptedMessageModel
{
    public required string PublicKey { get; set; }
    public required string Login { get; set; }
}