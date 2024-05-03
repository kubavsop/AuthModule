using System.Text.Json.Serialization;

namespace AuthModule.Shared.Models;

public sealed class EncryptedMessageModel
{
    [JsonPropertyName("encryptedMessage")]
    public required string EncryptedMessage { get; set; }
}