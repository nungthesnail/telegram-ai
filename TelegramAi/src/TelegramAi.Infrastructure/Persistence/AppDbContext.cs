using Microsoft.EntityFrameworkCore;
using TelegramAi.Domain.Entities;

namespace TelegramAi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<ChannelBotLink> ChannelBotLinks => Set<ChannelBotLink>();
    public DbSet<Dialog> Dialogs => Set<Dialog>();
    public DbSet<DialogMessage> DialogMessages => Set<DialogMessage>();
    public DbSet<ChannelPost> ChannelPosts => Set<ChannelPost>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).IsRequired().HasMaxLength(256);
            entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(128);
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.Property(x => x.Title).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.TelegramLink).HasMaxLength(500);

            entity.HasOne(x => x.Owner)
                .WithMany(x => x.Channels)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.BotLink)
                .WithOne(x => x.Channel)
                .HasForeignKey<ChannelBotLink>(x => x.ChannelId);
        });

        modelBuilder.Entity<ChannelBotLink>(entity =>
        {
            entity.Property(x => x.VerificationCode).IsRequired().HasMaxLength(32);
            entity.HasIndex(x => x.VerificationCode).IsUnique();
        });

        modelBuilder.Entity<Dialog>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.HasOne(x => x.User)
                .WithMany(x => x.Dialogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Channel)
                .WithMany(x => x.Dialogs)
                .HasForeignKey(x => x.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DialogMessage>(entity =>
        {
            entity.Property(x => x.Content).IsRequired().HasMaxLength(4000);
            entity.HasOne(x => x.Dialog)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.DialogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChannelPost>(entity =>
        {
            entity.Property(x => x.Content).IsRequired().HasMaxLength(8000);
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.HasOne(x => x.Channel)
                .WithMany(x => x.Posts)
                .HasForeignKey(x => x.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Currency).IsRequired().HasMaxLength(16);
            entity.HasOne(x => x.User)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}


