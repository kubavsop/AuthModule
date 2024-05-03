namespace AuthModule.Client.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public required string Login { get; set; }
    public required string PublicKey { get; set; }
    public required string EncryptedPrivateKey { get; set; }
}