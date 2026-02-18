using System;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Domain;

namespace TaskBoard.Infrastructure.Persistence;

public sealed class TaskBoardDbContext : DbContext
{

    public TaskBoardDbContext(DbContextOptions<TaskBoardDbContext> options) : base(options) {}

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        var b = modelBuilder.Entity<TaskItem>();

        b.ToTable("Tasks");
        b.HasKey(x => x.Id);

        b.Property(x => x.RowVersion).IsRowVersion();
        
        b.Property(x => x.Id).ValueGeneratedNever();

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

        b.Property(x => x.DueDate)
            .HasColumnType("datetime2(0)");

        
        b.Property<byte[]>("RowVersion")
            .IsRowVersion();

        
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
