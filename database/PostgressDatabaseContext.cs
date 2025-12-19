namespace api.Data;

using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using UserProfileModel;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<UserModel> Users { get; set; }
    public DbSet<ChatConversation> ChatConversations { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.Email)
            .IsUnique();


        modelBuilder.Entity<ChatMessage>()
            .HasIndex(u => u.Id)
            .IsUnique();


        modelBuilder.Entity<ChatConversation>()
            .HasIndex(u => u.Id)
            .IsUnique();

        modelBuilder.Entity<ChatConversation>()
            .HasIndex(u => u.UserId)
            .IsUnique();


    }
}