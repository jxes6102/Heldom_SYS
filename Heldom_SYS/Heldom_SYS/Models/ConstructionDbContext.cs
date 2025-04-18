using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Heldom_SYS.Models;

public partial class ConstructionDbContext : DbContext
{
    public ConstructionDbContext()
    {
    }

    public ConstructionDbContext(DbContextOptions<ConstructionDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accident> Accidents { get; set; }

    public virtual DbSet<AccidentFile> AccidentFiles { get; set; }

    public virtual DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    public virtual DbSet<Blueprint> Blueprints { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeDetail> EmployeeDetails { get; set; }

    public virtual DbSet<LeaveRecord> LeaveRecords { get; set; }

    public virtual DbSet<PrintCategory> PrintCategories { get; set; }

    public virtual DbSet<Temporarier> Temporariers { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseSqlServer("Name=ConnectionStrings:ConstructionDB");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accident>(entity =>
        {
            entity.HasKey(e => e.AccidentId).HasName("PK__Accident__8133DE8FA0FF7C26");

            entity.ToTable("Accident");

            entity.HasIndex(e => e.AccidentId, "UQ__Accident__8133DE8E38CDEED2").IsUnique();

            entity.Property(e => e.AccidentId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AccidentID");
            entity.Property(e => e.AccidentTitle).HasMaxLength(10);
            entity.Property(e => e.AccidentType).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.IncidentControllerId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("IncidentControllerID");
            entity.Property(e => e.Response).HasMaxLength(100);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.UploadTime).HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.Accidents)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accident_Employee");
        });

        modelBuilder.Entity<AccidentFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__Accident__6F0F989FD61AA56C");

            entity.ToTable("AccidentFile");

            entity.Property(e => e.FileId)
                .HasMaxLength(20)
                .HasColumnName("FileID");
            entity.Property(e => e.AccidentId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AccidentID");

            entity.HasOne(d => d.Accident).WithMany(p => p.AccidentFiles)
                .HasForeignKey(d => d.AccidentId)
                .HasConstraintName("FK_AccidentFile_Accident");
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69263C74C7CE41");

            entity.ToTable("AttendanceRecord");

            entity.Property(e => e.AttendanceId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AttendanceID");
            entity.Property(e => e.CheckInTime).HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.WorkDate).HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.AttendanceRecords)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AttendanceRecord_Employee");
        });

        modelBuilder.Entity<Blueprint>(entity =>
        {
            entity.HasKey(e => e.BlueprintId).HasName("PK__Blueprin__C12C6E55945AC7FA");

            entity.ToTable("Blueprint");

            entity.HasIndex(e => e.BlueprintId, "UQ__Blueprin__C12C6E542EC31ACD").IsUnique();

            entity.Property(e => e.BlueprintId)
                .HasMaxLength(20)
                .HasColumnName("BlueprintID");
            entity.Property(e => e.BlueprintName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.PrintCategoryId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("PrintCategoryID");
            entity.Property(e => e.UploadTime).HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.Blueprints)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blueprint_Employee");

            entity.HasOne(d => d.PrintCategory).WithMany(p => p.Blueprints)
                .HasForeignKey(d => d.PrintCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Blueprint_PrintCategory");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Company__2D971C4C01B2E0A6");

            entity.ToTable("Company");

            entity.HasIndex(e => e.CompanyName, "UQ__Company__9BCE05DC85E794C6").IsUnique();

            entity.Property(e => e.CompanyId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CompanyID");
            entity.Property(e => e.CompanyName).HasMaxLength(50);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF151BA4EB4");

            entity.ToTable("Employee");

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.HireDate).HasColumnType("datetime");
            entity.Property(e => e.Position).HasMaxLength(20);
            entity.Property(e => e.PositionRole).HasMaxLength(10);
            entity.Property(e => e.ResignationDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<EmployeeDetail>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF142D6CEBF");

            entity.ToTable("EmployeeDetail");

            entity.HasIndex(e => e.Mail, "UQ__Employee__2724B2D15B503D81").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "UQ__Employee__85FB4E385EC250FC").IsUnique();

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.BirthDate).HasColumnType("datetime");
            entity.Property(e => e.Department).HasMaxLength(20);
            entity.Property(e => e.EmergencyContact).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyRelationship).HasMaxLength(10);
            entity.Property(e => e.EmployeeName).HasMaxLength(20);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsFixedLength();
            entity.Property(e => e.ImmediateSupervisor)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Mail)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Employee).WithOne(p => p.EmployeeDetail)
                .HasForeignKey<EmployeeDetail>(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EmployeeDetail_Employee");
        });

        modelBuilder.Entity<LeaveRecord>(entity =>
        {
            entity.HasKey(e => new { e.EmployeeId, e.StartTime }).HasName("PK__LeaveRec__BB5DCF77B636D799");

            entity.ToTable("LeaveRecord");

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRecords)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LeaveRecord_Employee");
        });

        modelBuilder.Entity<PrintCategory>(entity =>
        {
            entity.HasKey(e => e.PrintCategoryId).HasName("PK__PrintCat__FDF824BE7274F3F5");

            entity.ToTable("PrintCategory");

            entity.HasIndex(e => e.PrintCategory1, "UQ__PrintCat__59630B5CCA90CB75").IsUnique();

            entity.Property(e => e.PrintCategoryId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("PrintCategoryID");
            entity.Property(e => e.PrintCategory1)
                .HasMaxLength(10)
                .HasColumnName("PrintCategory");
        });

        modelBuilder.Entity<Temporarier>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Temporar__7AD04FF10F319776");

            entity.ToTable("Temporarier");

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("EmployeeID");
            entity.Property(e => e.CompanyId)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("CompanyID");
            entity.Property(e => e.EmployeeName).HasMaxLength(20);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Company).WithMany(p => p.Temporariers)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Temporarier_Company");

            entity.HasOne(d => d.Employee).WithOne(p => p.Temporarier)
                .HasForeignKey<Temporarier>(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Temporarier_Employee");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
