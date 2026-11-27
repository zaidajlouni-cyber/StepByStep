using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StepByStep.Models;

public partial class StepByStepDbContext : DbContext
{
    public StepByStepDbContext()
    {
    }

    public StepByStepDbContext(DbContextOptions<StepByStepDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttendanceHomework> AttendanceHomeworks { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Parent> Parents { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentBehavior> StudentBehaviors { get; set; }

    public virtual DbSet<StudyPlan> StudyPlans { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WeeklyEvaluation> WeeklyEvaluations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendanceHomework>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__Attendan__FBDF78C91CDA73C0");

            entity.ToTable("Attendance_Homework");

            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.RecordDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.HasOne(d => d.Student).WithMany(p => p.AttendanceHomeworks)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Stude__4F7CD00D");

            entity.HasOne(d => d.Subject).WithMany(p => p.AttendanceHomeworks)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Subje__5070F446");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E32C3012057");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Parent).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Paren__59FA5E80");
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.ParentId).HasName("PK__Parents__D339510F0D8996D2");

            entity.Property(e => e.ParentId)
                .ValueGeneratedNever()
                .HasColumnName("ParentID");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.ParentNavigation).WithOne(p => p.Parent)
                .HasForeignKey<Parent>(d => d.ParentId)
                .HasConstraintName("FK__Parents__ParentI__3C69FB99");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A79D20F4F70");

            entity.Property(e => e.StudentId)
                .ValueGeneratedNever()
                .HasColumnName("StudentID");
            entity.Property(e => e.GradeLevel).HasMaxLength(50);
            entity.Property(e => e.ParentId).HasColumnName("ParentID");

            entity.HasOne(d => d.Parent).WithMany(p => p.Students)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__Parent__4316F928");

            entity.HasOne(d => d.StudentNavigation).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__Studen__4222D4EF");
        });

        modelBuilder.Entity<StudentBehavior>(entity =>
        {
            entity.HasKey(e => e.BehaviorId);

            entity.Property(e => e.BehaviorId).HasColumnName("BehaviorID");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");
            entity.Property(e => e.BehaviorType).HasMaxLength(30);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Student).WithMany()
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Teacher).WithMany()
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<StudyPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__Study_Pl__755C22D704DEE6E7");

            entity.ToTable("Study_Plans");

            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.PriorityLevel).HasMaxLength(20);
            entity.Property(e => e.RecommendedHours).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");

            entity.HasOne(d => d.Student).WithMany(p => p.StudyPlans)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Study_Pla__Stude__5441852A");

            entity.HasOne(d => d.Subject).WithMany(p => p.StudyPlans)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Study_Pla__Subje__5535A963");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subjects__AC1BA388397EA82F");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.SubjectName).HasMaxLength(100);
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Subjects__Teache__45F365D3");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.TeacherId).HasName("PK__Teachers__EDF25944EB96F8E0");

            entity.Property(e => e.TeacherId)
                .ValueGeneratedNever()
                .HasColumnName("TeacherID");
            entity.Property(e => e.Specialization).HasMaxLength(100);

            entity.HasOne(d => d.TeacherNavigation).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.TeacherId)
                .HasConstraintName("FK__Teachers__Teache__3F466844");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC2C8A1EB4");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053446845606").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<WeeklyEvaluation>(entity =>
        {
            entity.HasKey(e => e.EvaluationId).HasName("PK__Weekly_E__36AE68D3B41D5031");

            entity.ToTable("Weekly_Evaluations");

            entity.Property(e => e.EvaluationId).HasColumnName("EvaluationID");
            entity.Property(e => e.EvaluationDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Score).HasColumnType("decimal(3, 1)");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.TeacherNotes).HasMaxLength(255);

            entity.HasOne(d => d.Student).WithMany(p => p.WeeklyEvaluations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Weekly_Ev__Stude__4AB81AF0");

            entity.HasOne(d => d.Subject).WithMany(p => p.WeeklyEvaluations)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Weekly_Ev__Subje__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}