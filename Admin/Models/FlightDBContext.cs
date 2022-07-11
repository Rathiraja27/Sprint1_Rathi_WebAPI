using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Admin.Models
{
    public partial class FlightDBContext : DbContext
    {
        public FlightDBContext()
        {
        }

        public FlightDBContext(DbContextOptions<FlightDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Airline> Airlines { get; set; }
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Login> Logins { get; set; }
        public virtual DbSet<Passenger> Passengers { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=CTSDOTNET369;Initial Catalog=FlightDB; User Id = sa; Password = pass@word1");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Airline>(entity =>
            {
                entity.ToTable("Airline");

                entity.Property(e => e.Address).HasMaxLength(50);

                entity.Property(e => e.AirlineName).HasMaxLength(50);

                entity.Property(e => e.AirlineStatus).HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(50);

                entity.Property(e => e.Logo).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Booking");

                entity.Property(e => e.BookedOn).HasColumnType("datetime");

                entity.Property(e => e.CustomerName).HasMaxLength(50);

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.HasOne(d => d.Flight)
                    .WithMany(p => p.Bookings)
                    .HasForeignKey(d => d.FlightId)
                    .HasConstraintName("FK_Booking_Schedule");
            });

            modelBuilder.Entity<Login>(entity =>
            {
                entity.ToTable("Login");

                entity.Property(e => e.Mode).HasMaxLength(50);

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.UserName).HasMaxLength(50);
            });

            modelBuilder.Entity<Passenger>(entity =>
            {
                entity.ToTable("Passenger");

                entity.Property(e => e.Meal).HasMaxLength(50);

                entity.Property(e => e.PassengerName).HasMaxLength(50);

                entity.Property(e => e.Pnr)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Seat).HasMaxLength(10);

                entity.Property(e => e.Trip).HasMaxLength(50);

                entity.HasOne(d => d.Booking)
                    .WithMany(p => p.Passengers)
                    .HasForeignKey(d => d.BookingId)
                    .HasConstraintName("FK_Passenger_Booking");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);

                entity.ToTable("RefreshToken");

                entity.Property(e => e.RefreshedToken).HasMaxLength(100);

                entity.Property(e => e.Token).HasMaxLength(300);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_RefreshToken_Login");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.FlightId);

                entity.ToTable("Schedule");

                entity.Property(e => e.EndDateTime).HasColumnType("datetime");

                entity.Property(e => e.FlightName).HasMaxLength(50);

                entity.Property(e => e.FromPlace).HasMaxLength(50);

                entity.Property(e => e.InstrumentUsed).HasMaxLength(50);

                entity.Property(e => e.Meal).HasMaxLength(50);

                entity.Property(e => e.SceduledDays).HasMaxLength(50);

                entity.Property(e => e.StartDateTime).HasColumnType("datetime");

                entity.Property(e => e.ToPlace).HasMaxLength(50);

                entity.HasOne(d => d.Airline)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.AirlineId)
                    .HasConstraintName("FK_Schedule_Airline");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
