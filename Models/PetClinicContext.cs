using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PetClinicSystem.Models;

public partial class PetClinicContext : DbContext
{
    public PetClinicContext()
    {
    }

    public PetClinicContext(DbContextOptions<PetClinicContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<BillingDetail> BillingDetails { get; set; }

    public virtual DbSet<Consultation> Consultations { get; set; }

    public virtual DbSet<ConsultationTreatment> ConsultationTreatments { get; set; }

    public virtual DbSet<DiagnosticTest> DiagnosticTests { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<Treatment> Treatments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vaccination> Vaccinations { get; set; }

    public virtual DbSet<VaccineRecord> VaccineRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=PetClinic;Trusted_Connection=True;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC27D42D763");

            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Duration).HasDefaultValue(30);
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Scheduled");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Patie__45F365D3");

            entity.HasOne(d => d.Vet).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.VetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__VetId__46E78A0C");
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__Billing__11F2FC6AA0A67081");

            entity.ToTable("Billing");

            entity.Property(e => e.Balance)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BillDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.PaidAmount)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Billings)
                .HasForeignKey(d => d.AppointmentId)
                .HasConstraintName("FK__Billing__Appoint__6FE99F9F");

            entity.HasOne(d => d.Consultation).WithMany(p => p.Billings)
                .HasForeignKey(d => d.ConsultationId)
                .HasConstraintName("FK__Billing__Consult__6EF57B66");
        });

        modelBuilder.Entity<BillingDetail>(entity =>
        {
            entity.HasKey(e => e.BillingDetailId).HasName("PK__BillingD__77F5CC249D2CAB3B");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ItemType).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Bill).WithMany(p => p.BillingDetails)
                .HasForeignKey(d => d.BillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BillingDe__BillI__73BA3083");
        });

        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.ConsultationId).HasName("PK__Consulta__5D014A98732C44DD");

            entity.Property(e => e.ConsultationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FollowUpDate).HasColumnType("datetime");
            entity.Property(e => e.IsFollowUp).HasDefaultValue(false);
            entity.Property(e => e.Temperature).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Appointment).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Consultat__Appoi__4BAC3F29");

            entity.HasOne(d => d.Patient).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Consultat__Patie__4D94879B");

            entity.HasOne(d => d.Vet).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.VetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Consultat__VetId__4CA06362");
        });

        modelBuilder.Entity<ConsultationTreatment>(entity =>
        {
            entity.HasKey(e => e.ConsultationTreatmentId).HasName("PK__Consulta__F92DEA6C6038585F");

            entity.Property(e => e.Cost).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Consultation).WithMany(p => p.ConsultationTreatments)
                .HasForeignKey(d => d.ConsultationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Consultat__Consu__619B8048");

            entity.HasOne(d => d.Treatment).WithMany(p => p.ConsultationTreatments)
                .HasForeignKey(d => d.TreatmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Consultat__Treat__628FA481");
        });

        modelBuilder.Entity<DiagnosticTest>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PK__Diagnost__8CC331604098DFAF");

            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TestName).HasMaxLength(100);
            entity.Property(e => e.TestType).HasMaxLength(50);

            entity.HasOne(d => d.Consultation).WithMany(p => p.DiagnosticTests)
                .HasForeignKey(d => d.ConsultationId)
                .HasConstraintName("FK__Diagnosti__Consu__5AEE82B9");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK__Owners__819385B855F4E3CC");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.ZipCode).HasMaxLength(20);
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patients__970EC366A442AEC0");

            entity.Property(e => e.Breed).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MicrochipId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PhotoPath).HasMaxLength(255);
            entity.Property(e => e.Species).HasMaxLength(50);

            entity.HasOne(d => d.Owner).WithMany(p => p.Patients)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Patients__OwnerI__403A8C7D");
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.PrescriptionId).HasName("PK__Prescrip__40130832D9C8979C");

            entity.Property(e => e.Dosage).HasMaxLength(50);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.Frequency).HasMaxLength(50);
            entity.Property(e => e.IsDispensed).HasDefaultValue(false);
            entity.Property(e => e.MedicationName).HasMaxLength(100);
            entity.Property(e => e.PrescribedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Refills).HasDefaultValue(0);

            entity.HasOne(d => d.Consultation).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.ConsultationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Consu__68487DD7");
        });

        modelBuilder.Entity<Treatment>(entity =>
        {
            entity.HasKey(e => e.TreatmentId).HasName("PK__Treatmen__1A57B7F1195FF678");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.DefaultCost)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C355EE6FF");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasKey(e => e.VaccineId).HasName("PK__Vaccinat__45DC6889254041C8");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsCore).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RecommendedSchedule).HasMaxLength(255);
            entity.Property(e => e.Species).HasMaxLength(50);
        });

        modelBuilder.Entity<VaccineRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PK__VaccineR__FBDF78E9394DE4DD");

            entity.Property(e => e.LotNumber).HasMaxLength(50);

            entity.HasOne(d => d.AdministeredByNavigation).WithMany(p => p.VaccineRecords)
                .HasForeignKey(d => d.AdministeredBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VaccineRe__Admin__5629CD9C");

            entity.HasOne(d => d.Patient).WithMany(p => p.VaccineRecords)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VaccineRe__Patie__5535A963");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.VaccineRecords)
                .HasForeignKey(d => d.VaccineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__VaccineRe__Vacci__5441852A");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
