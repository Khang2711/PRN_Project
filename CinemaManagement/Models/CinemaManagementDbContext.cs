using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace CinemaManagement.Models;

public partial class CinemaManagementDbContext : DbContext
{
    public CinemaManagementDbContext()
    {
    }

    public CinemaManagementDbContext(DbContextOptions<CinemaManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingDetail> BookingDetails { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //Tạo phương thức lấy chuỗi kết nối từ file appsettings.json
    private string GetConnectionString()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsetting.json", true, true)
            .Build();
        var connectionstring = configuration["ConnectionStrings:DefaultConnection"];
        return connectionstring;
    }


    //Override lại phương thức OnConfiguring để thiết lập kết nối cho CSDL
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(e => new { e.CustomerId, e.BookingTime }, "IX_Bookings_Customer_BookingTime").IsDescending(false, true);

            entity.HasIndex(e => e.BookingCode, "UQ_Bookings_BookingCode").IsUnique();

            entity.HasIndex(e => new { e.BookingId, e.ShowtimeId }, "UQ_Bookings_Booking_Showtime").IsUnique();

            entity.Property(e => e.BookingCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.BookingTime)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaidAt).HasPrecision(0);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.ConfirmedByUser).WithMany(p => p.BookingConfirmedByUsers)
                .HasForeignKey(d => d.ConfirmedByUserId)
                .HasConstraintName("FK_Bookings_ConfirmedBy");

            entity.HasOne(d => d.Customer).WithMany(p => p.BookingCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Customer");

            entity.HasOne(d => d.Showtime).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ShowtimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bookings_Showtime");
        });

        modelBuilder.Entity<BookingDetail>(entity =>
        {
            entity.HasIndex(e => e.BookingId, "IX_BookingDetails_BookingId");

            entity.HasIndex(e => new { e.ShowtimeId, e.SeatId }, "UX_BookingDetails_ActiveSeatPerShowtime")
                .IsUnique()
                .HasFilter("([IsCancelled]=(0))");

            entity.Property(e => e.CheckedInAt).HasPrecision(0);
            entity.Property(e => e.PriceAtBooking).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CheckedInByUser).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.CheckedInByUserId)
                .HasConstraintName("FK_BookingDetails_CheckedInBy");

            entity.HasOne(d => d.Seat).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDetails_Seat");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingDetails)
                .HasPrincipalKey(p => new { p.BookingId, p.ShowtimeId })
                .HasForeignKey(d => new { d.BookingId, d.ShowtimeId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDetails_BookingShowtime");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.Property(e => e.AgeRating).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Genre).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.PosterUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(e => e.RoomName, "UQ_Rooms_RoomName").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoomName).HasMaxLength(50);
            entity.Property(e => e.RoomType).HasMaxLength(50);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasIndex(e => new { e.RoomId, e.SeatRow, e.SeatNumber }, "UQ_Seats_Room_Row_Number").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SeatRow).HasMaxLength(5);
            entity.Property(e => e.SeatType)
                .HasMaxLength(20)
                .HasDefaultValue("Standard");

            entity.HasOne(d => d.Room).WithMany(p => p.Seats)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seats_Rooms");
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasIndex(e => new { e.MovieId, e.StartTime }, "IX_Showtimes_Movie_StartTime");

            entity.HasIndex(e => new { e.RoomId, e.StartTime }, "IX_Showtimes_Room_StartTime");

            entity.HasIndex(e => new { e.ShowtimeId, e.RoomId }, "UQ_Showtimes_Showtime_Room").IsUnique();

            entity.Property(e => e.EndTime).HasPrecision(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StartTime).HasPrecision(0);
            entity.Property(e => e.TicketPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Movie).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Showtimes_Movies");

            entity.HasOne(d => d.Room).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Showtimes_Rooms");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

    }

}
