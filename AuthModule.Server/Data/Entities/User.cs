namespace AuthModule.Server.Data.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public required string Login { get; set; }
    public required string PublicKey { get; set; }
}