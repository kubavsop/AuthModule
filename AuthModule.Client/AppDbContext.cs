using AuthModule.Client.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthModule.Client;

public class AppDbContext: DbContext
{
    public DbSet<User> Users { get; init; }
    public AppDbContext(DbContextOptions options): base(options) {}
}