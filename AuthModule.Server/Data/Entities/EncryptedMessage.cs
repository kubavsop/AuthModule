namespace AuthModule.Server.Data.Entities;

public class EncryptedMessage
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
    public User User { get; set; } = null!;
    public Guid UserId { get; set; }
    
    public DateTime? MessageExpirationTime { get; set; }
    
    public bool MessageIsExpired => DateTime.UtcNow > MessageExpirationTime;
}