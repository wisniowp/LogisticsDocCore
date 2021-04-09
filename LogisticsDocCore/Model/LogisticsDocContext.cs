using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace LogisticsDocCore.Model
{
    public partial class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        //private DbSet<User> users;

        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        //public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        //public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        //public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        //public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
        public virtual DbSet<Doc> Docs { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistories { get; set; }
        public virtual DbSet<Organizer> Organizers { get; set; }
        public virtual DbSet<User> AppUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=LT09543\\SQLEXPRESS;Database=LogisticsDoc;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<AspNetRole>(entity =>
            //{
            //    entity.Property(e => e.Id).HasMaxLength(128);

            //    entity.Property(e => e.Name).IsRequired();
            //});

            //modelBuilder.Entity<AspNetUser>(entity =>
            //{
            //    entity.Property(e => e.Id).HasMaxLength(128);

            //    entity.Property(e => e.Discriminator)
            //        .IsRequired()
            //        .HasMaxLength(128);
            //});

            //modelBuilder.Entity<AspNetUserClaim>(entity =>
            //{
            //    entity.Property(e => e.UserId)
            //        .IsRequired()
            //        .HasMaxLength(128)
            //        .HasColumnName("User_Id");

            //    entity.HasOne(d => d.User)
            //        .WithMany(p => p.AspNetUserClaims)
            //        .HasForeignKey(d => d.UserId)
            //        .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_User_Id");
            //});

            //modelBuilder.Entity<AspNetUserLogin>(entity =>
            //{
            //    entity.HasKey(e => new { e.UserId, e.LoginProvider, e.ProviderKey })
            //        .HasName("PK_dbo.AspNetUserLogins");

            //    entity.Property(e => e.UserId).HasMaxLength(128);

            //    entity.Property(e => e.LoginProvider).HasMaxLength(128);

            //    entity.Property(e => e.ProviderKey).HasMaxLength(128);

            //    entity.HasOne(d => d.User)
            //        .WithMany(p => p.AspNetUserLogins)
            //        .HasForeignKey(d => d.UserId)
            //        .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId");
            //});

            //modelBuilder.Entity<AspNetUserRole>(entity =>
            //{
            //    entity.HasKey(e => new { e.UserId, e.RoleId })
            //        .HasName("PK_dbo.AspNetUserRoles");

            //    entity.Property(e => e.UserId).HasMaxLength(128);

            //    entity.Property(e => e.RoleId).HasMaxLength(128);

            //    entity.HasOne(d => d.Role)
            //        .WithMany(p => p.AspNetUserRoles)
            //        .HasForeignKey(d => d.RoleId)
            //        .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId");

            //    entity.HasOne(d => d.User)
            //        .WithMany(p => p.AspNetUserRoles)
            //        .HasForeignKey(d => d.UserId)
            //        .HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId");
            //});

            modelBuilder.Entity<Doc>(entity =>
            {
                entity.HasKey(e => e.DocsId)
                    .HasName("PK_dbo.Docs");

                entity.HasIndex(e => e.CreatedByLogin, "IX_CreatedByLogin");

                entity.HasIndex(e => e.OrganizerEvent, "IX_OrganizerEvent");

                entity.Property(e => e.CreatedByLogin).HasMaxLength(50);

                entity.Property(e => e.DocsName).HasMaxLength(50);

                entity.Property(e => e.InvoiceNo).HasMaxLength(50);

                entity.Property(e => e.OrganizerEvent).HasMaxLength(128);

                entity.Property(e => e.StausChangeDateTime).HasColumnType("datetime");

                entity.HasOne(d => d.CreatedByLoginNavigation)
                    .WithMany(p => p.Docs)
                    .HasForeignKey(d => d.CreatedByLogin)
                    .HasConstraintName("FK_dbo.Docs_dbo.Users_CreatedByLogin");

                entity.HasOne(d => d.OrganizerEventNavigation)
                    .WithMany(p => p.Docs)
                    .HasForeignKey(d => d.OrganizerEvent)
                    .HasConstraintName("FK_dbo.Docs_dbo.Organizers_OrganizerEvent");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");

                entity.ToTable("__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<Organizer>(entity =>
            {
                entity.HasKey(e => e.Organizer1)
                    .HasName("PK_dbo.Organizers");

                entity.Property(e => e.Organizer1)
                    .HasMaxLength(128)
                    .HasColumnName("Organizer");

                entity.Property(e => e.OrganizerAddress).HasMaxLength(50);

                entity.Property(e => e.OrganizerCity).HasMaxLength(50);

                entity.Property(e => e.OrganizerName).IsRequired();

                entity.Property(e => e.OrganizerPostCode).HasMaxLength(10);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Login)
                    .HasName("PK_dbo.Users");

                entity.HasIndex(e => e.LinkedToOrganizer, "IX_LinkedToOrganizer");

                entity.Property(e => e.Login).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.LinkedToOrganizer).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(30);

                entity.Property(e => e.Password).HasMaxLength(100);

                entity.Property(e => e.Surname).HasMaxLength(50);

                entity.HasOne(d => d.LinkedToOrganizerNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.LinkedToOrganizer)
                    .HasConstraintName("FK_dbo.Users_dbo.Organizers_LinkedToOrganizer");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
