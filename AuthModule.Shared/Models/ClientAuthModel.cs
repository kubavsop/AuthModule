namespace AuthModule.Shared.Models;

public sealed class ClientAuthModel
{
    public required string Login { get; set; }
    public required string Passphrase { get; set; }
}