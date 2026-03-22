using System;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain;

namespace TaskBoard.Infrastructure.Persistence;

public sealed class TaskBoardDbContext : DbContext
{

    public TaskBoardDbContext(DbContextOptions<TaskBoardDbContext> options) : base(options) {}

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        var u = modelBuilder.Entity<User>();

        u.ToTable("Users");
        u.HasKey(x => x.Id);

        u.Property(x => x.Id)
            .HasMaxLength(32)
            .ValueGeneratedNever();

        u.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        u.HasIndex(x => x.Email)
            .IsUnique();

        u.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        u.Property(x => x.CreatedAt)
            .HasColumnType("datetime2(0)")
            .IsRequired();

        u.Property(x => x.IsAdmin)
            .IsRequired();

        var b = modelBuilder.Entity<TaskItem>();

        b.ToTable("Tasks");
        b.HasKey(x => x.Id);

        b.Property(x => x.RowVersion).IsRowVersion();
        
        b.Property(x => x.Id).ValueGeneratedNever();

        b.HasIndex(x => x.OwnerId);

        b.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Description)
            .HasColumnType("nvarchar(max)");

        
        b.Property(x => x.Priority)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        b.Property(x => x.Status)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .HasColumnType("datetime2(0)")
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2(0)");

        b.Property(x => x.DueDate)
            .HasColumnType("datetime2(0)");

        b.Property(x => x.LastCheckedAt)
            .HasColumnType("datetime2(0)");

        b.Property(x => x.OwnerId)
            .HasMaxLength(32)
            .IsRequired();

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);

        
        b.OwnsMany(x => x.Tags, tb =>
        {

            tb.ToTable("TaskTags");

            tb.WithOwner().HasForeignKey("TaskId");

            tb.Property(t => t.Value)
                .HasColumnName("Tag")
                .HasMaxLength(20)
                .IsRequired();

            tb.HasKey("TaskId", nameof(Tag.Value));
            tb.HasIndex(nameof(Tag.Value));
            
        });

        b.Navigation(x => x.Tags).AutoInclude();

        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.Priority);

    }



}
