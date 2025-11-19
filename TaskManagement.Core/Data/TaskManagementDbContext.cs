using Microsoft.EntityFrameworkCore;
using TaskManagement.Core.Models;

namespace TaskManagement.Core.Data
{
    public class TaskManagementDbContext : DbContext
    {
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> Tasks { get; set; } = null!;
        public DbSet<TaskFile> TaskFiles { get; set; } = null!;
        public DbSet<TaskIssue> TaskIssues { get; set; } = null!;
        public DbSet<TaskDependency> TaskDependencies { get; set; } = null!;
        public DbSet<TaskContext> TaskContexts { get; set; } = null!;
        public DbSet<TaskIteration> TaskIterations { get; set; } = null!;
        public DbSet<TaskReference> TaskReferences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TaskItem entity
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CompletionPercentage).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();

                // Configure subtasks (self-referencing relationship)
                entity.HasOne(e => e.ParentTask)
                    .WithMany(e => e.SubTasks)
                    .HasForeignKey(e => e.ParentTaskId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ModifiedFiles)
                    .WithOne(e => e.Task)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Issues)
                    .WithOne(e => e.Task)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure task dependencies
                entity.HasMany(e => e.Dependencies)
                    .WithOne(e => e.DependentTask)
                    .HasForeignKey(e => e.DependentTaskId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Dependents)
                    .WithOne(e => e.DependsOnTask)
                    .HasForeignKey(e => e.DependsOnTaskId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure context relationship (one-to-one)
                entity.HasOne(e => e.Context)
                    .WithOne(e => e.Task)
                    .HasForeignKey<TaskContext>(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure iterations
                entity.HasMany(e => e.Iterations)
                    .WithOne(e => e.Task)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure references
                entity.HasMany(e => e.References)
                    .WithOne(e => e.Task)
                    .HasForeignKey(e => e.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TaskFile entity
            modelBuilder.Entity<TaskFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ChangeDescription).HasMaxLength(1000);
                entity.Property(e => e.ModifiedAt).IsRequired();
            });

            // Configure TaskIssue entity
            modelBuilder.Entity<TaskIssue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Configure TaskDependency entity
            modelBuilder.Entity<TaskDependency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).IsRequired();

                // Ensure a task cannot depend on itself
                entity.HasIndex(e => new { e.DependentTaskId, e.DependsOnTaskId }).IsUnique();
            });

            // Configure TaskContext entity
            modelBuilder.Entity<TaskContext>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OriginalRequest).IsRequired();
                entity.Property(e => e.ConversationHistory).HasColumnType("TEXT");
                entity.Property(e => e.AdditionalNotes).HasColumnType("TEXT");
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            // Configure TaskIteration entity
            modelBuilder.Entity<TaskIteration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IterationNumber).IsRequired();
                entity.Property(e => e.WhatWasTried).IsRequired().HasColumnType("TEXT");
                entity.Property(e => e.Outcome).IsRequired();
                entity.Property(e => e.LessonsLearned).HasColumnType("TEXT");
                entity.Property(e => e.FilesModified).HasColumnType("TEXT");
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Configure TaskReference entity
            modelBuilder.Entity<TaskReference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReferenceType).IsRequired();
                entity.Property(e => e.Content).IsRequired().HasColumnType("TEXT");
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.OriginalFileName).HasMaxLength(255);
                entity.Property(e => e.MimeType).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}
