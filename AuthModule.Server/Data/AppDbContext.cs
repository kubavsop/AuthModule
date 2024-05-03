using AuthModule.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Server.Data;

public class AppDbContext: DbContext
{
    public DbSet<User> Users { get; init; }
    public DbSet<EncryptedMessage> Messages { get; init; }
    
    public AppDbContext(DbContextOptions options): base(options) {}
}