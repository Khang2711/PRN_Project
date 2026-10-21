
using System;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Data
{
    public class CinemaDbContext : DbContext
    {
        // Cac bang trong SQL Server
        public DbSet<DbMovie> Movies { get; set; }
        public DbSet<DbRoom> Rooms { get; set; }
        public DbSet<DbShowtime> Showtimes { get; set; }

        public CinemaDbContext()
        {
        }

        public CinemaDbContext(
            DbContextOptions<CinemaDbContext> options)
            : base(options)
        {
        }

        // Ket noi SQL Server
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString =
                    Environment.GetEnvironmentVariable(
                        "CINEMA_DB_CONNECTION")
                    ??
                    "Server=HoangTran;" +
                    "Database=CinemaManagementDB;" +
                    "Integrated Security=True;" +
                    "TrustServerCertificate=True;";

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        // Anh xa cac bang SQL
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Movies
            modelBuilder.Entity<DbMovie>(entity =>
            {
                entity.ToTable("Movies", "dbo");

                entity.HasKey(m => m.MovieId);

                entity.Property(m => m.MovieId)
                      .ValueGeneratedOnAdd();

                entity.Property(m => m.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(m => m.DurationMinutes)
                      .HasColumnType("smallint");
            });

            // Rooms
            modelBuilder.Entity<DbRoom>(entity =>
            {
                entity.ToTable("Rooms", "dbo");

                entity.HasKey(r => r.RoomId);

                entity.Property(r => r.RoomId)
                      .ValueGeneratedOnAdd();

                entity.Property(r => r.RoomName)
                      .HasMaxLength(50)
                      .IsRequired();
            });

            // Showtimes
            modelBuilder.Entity<DbShowtime>(entity =>
            {
                entity.ToTable("Showtimes", "dbo");

                entity.HasKey(s => s.ShowtimeId);

                entity.Property(s => s.ShowtimeId)
                      .ValueGeneratedOnAdd();

                entity.Property(s => s.StartTime)
                      .HasColumnType("datetime2(0)");

                entity.Property(s => s.EndTime)
                      .HasColumnType("datetime2(0)");

                entity.Property(s => s.TicketPrice)
                      .HasPrecision(10, 2);

                entity.Property(s => s.IsActive)
                      .HasDefaultValue(true);

                // Khoa ngoai den Movies
                entity.HasOne<DbMovie>()
                      .WithMany()
                      .HasForeignKey(s => s.MovieId);

                // Khoa ngoai den Rooms
                entity.HasOne<DbRoom>()
                      .WithMany()
                      .HasForeignKey(s => s.RoomId);
            });
        }

        // Kiem tra ket noi
        public bool TestConnection()
        {
            try
            {
                return Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }
    }

    // Entity bang Movies
    public class DbMovie
    {
        public int MovieId { get; set; }

        public string Title { get; set; } = "";

        public short DurationMinutes { get; set; }

        public bool IsActive { get; set; }
    }

    // Entity bang Rooms
    public class DbRoom
    {
        public int RoomId { get; set; }

        public string RoomName { get; set; } = "";

        public bool IsActive { get; set; }
    }

    // Entity bang Showtimes
    public class DbShowtime
    {
        public int ShowtimeId { get; set; }

        public int MovieId { get; set; }

        public int RoomId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal TicketPrice { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
