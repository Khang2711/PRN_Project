/*
    Cinema Management System - Database Script
    Based on Report 4 - Software Design Document
    DBMS: Microsoft SQL Server
    Scope: Single cinema branch, counter payment only, 7 tables.
*/

IF DB_ID(N'CinemaManagementDB') IS NULL
    CREATE DATABASE CinemaManagementDB;
GO

USE CinemaManagementDB;
GO

/* Drop tables in dependency order so the script can be rerun. */
IF OBJECT_ID(N'dbo.BookingDetails', N'U') IS NOT NULL DROP TABLE dbo.BookingDetails;
IF OBJECT_ID(N'dbo.Bookings', N'U') IS NOT NULL DROP TABLE dbo.Bookings;
IF OBJECT_ID(N'dbo.Showtimes', N'U') IS NOT NULL DROP TABLE dbo.Showtimes;
IF OBJECT_ID(N'dbo.Seats', N'U') IS NOT NULL DROP TABLE dbo.Seats;
IF OBJECT_ID(N'dbo.Rooms', N'U') IS NOT NULL DROP TABLE dbo.Rooms;
IF OBJECT_ID(N'dbo.Movies', N'U') IS NOT NULL DROP TABLE dbo.Movies;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* 1. Users: Customer, Staff, Admin. Guest users are not stored. */
CREATE TABLE dbo.Users (
    UserId              INT IDENTITY(1,1) NOT NULL,
    Username            NVARCHAR(50) NOT NULL,
    PasswordHash        NVARCHAR(255) NOT NULL,
    FullName            NVARCHAR(100) NOT NULL,
    Email               NVARCHAR(255) NULL,
    Phone               NVARCHAR(20) NULL,
    Role                VARCHAR(20) NOT NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt           DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Customer', 'Staff', 'Admin'))
);
GO

/* 2. Movies: movie catalogue and details. */
CREATE TABLE dbo.Movies (
    MovieId             INT IDENTITY(1,1) NOT NULL,
    Title               NVARCHAR(200) NOT NULL,
    Description         NVARCHAR(MAX) NULL,
    DurationMinutes     SMALLINT NOT NULL,
    Genre               NVARCHAR(100) NULL,
    AgeRating           NVARCHAR(20) NULL,
    ReleaseDate         DATE NULL,
    Language            NVARCHAR(50) NULL,
    PosterUrl           NVARCHAR(500) NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_Movies_IsActive DEFAULT (1),
    CreatedAt           DATETIME2(0) NOT NULL CONSTRAINT DF_Movies_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Movies PRIMARY KEY (MovieId),
    CONSTRAINT CK_Movies_Duration CHECK (DurationMinutes > 0)
);
GO

/* 3. Rooms: screening rooms for one cinema branch. */
CREATE TABLE dbo.Rooms (
    RoomId              INT IDENTITY(1,1) NOT NULL,
    RoomName            NVARCHAR(50) NOT NULL,
    RoomType            NVARCHAR(50) NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_Rooms_IsActive DEFAULT (1),
    CONSTRAINT PK_Rooms PRIMARY KEY (RoomId),
    CONSTRAINT UQ_Rooms_RoomName UNIQUE (RoomName)
);
GO

/* 4. Seats: a seat belongs to exactly one room. */
CREATE TABLE dbo.Seats (
    SeatId              INT IDENTITY(1,1) NOT NULL,
    RoomId              INT NOT NULL,
    SeatRow             NVARCHAR(5) NOT NULL,
    SeatNumber          INT NOT NULL,
    SeatType            NVARCHAR(20) NOT NULL CONSTRAINT DF_Seats_SeatType DEFAULT ('Standard'),
    IsActive            BIT NOT NULL CONSTRAINT DF_Seats_IsActive DEFAULT (1),
    CONSTRAINT PK_Seats PRIMARY KEY (SeatId),
    CONSTRAINT FK_Seats_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId),
    CONSTRAINT UQ_Seats_Room_Row_Number UNIQUE (RoomId, SeatRow, SeatNumber),
    CONSTRAINT CK_Seats_SeatNumber CHECK (SeatNumber > 0),
    CONSTRAINT CK_Seats_SeatType CHECK (SeatType IN ('Standard', 'VIP', 'Couple'))
);
GO

/* 5. Showtimes: one movie and one room per screening interval. */
CREATE TABLE dbo.Showtimes (
    ShowtimeId          INT IDENTITY(1,1) NOT NULL,
    MovieId             INT NOT NULL,
    RoomId              INT NOT NULL,
    StartTime           DATETIME2(0) NOT NULL,
    EndTime             DATETIME2(0) NOT NULL,
    TicketPrice         DECIMAL(10,2) NOT NULL,
    IsActive            BIT NOT NULL CONSTRAINT DF_Showtimes_IsActive DEFAULT (1),
    CONSTRAINT PK_Showtimes PRIMARY KEY (ShowtimeId),
    CONSTRAINT FK_Showtimes_Movies FOREIGN KEY (MovieId) REFERENCES dbo.Movies(MovieId),
    CONSTRAINT FK_Showtimes_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId),
    CONSTRAINT UQ_Showtimes_Showtime_Room UNIQUE (ShowtimeId, RoomId),
    CONSTRAINT CK_Showtimes_Interval CHECK (EndTime > StartTime),
    CONSTRAINT CK_Showtimes_TicketPrice CHECK (TicketPrice >= 0)
);
GO

/* 6. Bookings: one booking belongs to one customer and one showtime.
   Payment is recorded at the counter by staff; no online payment gateway. */
CREATE TABLE dbo.Bookings (
    BookingId           INT IDENTITY(1,1) NOT NULL,
    BookingCode         VARCHAR(30) NOT NULL,
    CustomerId          INT NOT NULL,
    ShowtimeId          INT NOT NULL,
    BookingTime         DATETIME2(0) NOT NULL CONSTRAINT DF_Bookings_BookingTime DEFAULT (SYSDATETIME()),
    Status              VARCHAR(20) NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT ('Pending'),
    IsCancelled         BIT NOT NULL CONSTRAINT DF_Bookings_IsCancelled DEFAULT (0),
    PaymentStatus       VARCHAR(20) NOT NULL CONSTRAINT DF_Bookings_PaymentStatus DEFAULT ('Unpaid'),
    PaidAmount          DECIMAL(10,2) NULL,
    PaidAt              DATETIME2(0) NULL,
    ConfirmedByUserId   INT NULL,
    Note                NVARCHAR(500) NULL,
    CONSTRAINT PK_Bookings PRIMARY KEY (BookingId),
    CONSTRAINT UQ_Bookings_BookingCode UNIQUE (BookingCode),
    /* Supports the composite FK from BookingDetails. */
    CONSTRAINT UQ_Bookings_Booking_Showtime UNIQUE (BookingId, ShowtimeId),
    CONSTRAINT FK_Bookings_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Bookings_Showtime FOREIGN KEY (ShowtimeId) REFERENCES dbo.Showtimes(ShowtimeId),
    CONSTRAINT FK_Bookings_ConfirmedBy FOREIGN KEY (ConfirmedByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_Bookings_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')),
    CONSTRAINT CK_Bookings_PaymentStatus CHECK (PaymentStatus IN ('Unpaid', 'Paid')),
    CONSTRAINT CK_Bookings_PaidAmount CHECK (PaidAmount IS NULL OR PaidAmount >= 0),
    CONSTRAINT CK_Bookings_PaymentConfirmation CHECK (
        (PaymentStatus = 'Unpaid' AND ConfirmedByUserId IS NULL AND PaidAt IS NULL)
        OR
        (PaymentStatus = 'Paid' AND ConfirmedByUserId IS NOT NULL AND PaidAt IS NOT NULL)
    )
);
GO

/* 7. BookingDetails: one row per ticket/seat, preserving cancelled history. */
CREATE TABLE dbo.BookingDetails (
    BookingDetailId     INT IDENTITY(1,1) NOT NULL,
    BookingId           INT NOT NULL,
    ShowtimeId          INT NOT NULL,
    SeatId              INT NOT NULL,
    PriceAtBooking      DECIMAL(10,2) NOT NULL,
    IsCancelled         BIT NOT NULL CONSTRAINT DF_BookingDetails_IsCancelled DEFAULT (0),
    CheckedIn           BIT NOT NULL CONSTRAINT DF_BookingDetails_CheckedIn DEFAULT (0),
    CheckedInAt         DATETIME2(0) NULL,
    CheckedInByUserId   INT NULL,
    CONSTRAINT PK_BookingDetails PRIMARY KEY (BookingDetailId),
    CONSTRAINT FK_BookingDetails_BookingShowtime
        FOREIGN KEY (BookingId, ShowtimeId)
        REFERENCES dbo.Bookings(BookingId, ShowtimeId),
    CONSTRAINT FK_BookingDetails_Seat FOREIGN KEY (SeatId) REFERENCES dbo.Seats(SeatId),
    CONSTRAINT FK_BookingDetails_CheckedInBy
        FOREIGN KEY (CheckedInByUserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT CK_BookingDetails_Price CHECK (PriceAtBooking >= 0),
    CONSTRAINT CK_BookingDetails_CheckIn CHECK (
        (CheckedIn = 0 AND CheckedInAt IS NULL AND CheckedInByUserId IS NULL)
        OR
        (CheckedIn = 1 AND CheckedInAt IS NOT NULL AND CheckedInByUserId IS NOT NULL)
    )
);
GO

/* A seat cannot be sold twice for the same showtime while the ticket is active.
   Cancelled records remain in the database as history. */
CREATE UNIQUE INDEX UX_BookingDetails_ActiveSeatPerShowtime
ON dbo.BookingDetails (ShowtimeId, SeatId)
WHERE IsCancelled = 0;
GO

CREATE INDEX IX_Showtimes_Movie_StartTime ON dbo.Showtimes(MovieId, StartTime);
CREATE INDEX IX_Showtimes_Room_StartTime ON dbo.Showtimes(RoomId, StartTime);
CREATE INDEX IX_Bookings_Customer_BookingTime ON dbo.Bookings(CustomerId, BookingTime DESC);
CREATE INDEX IX_BookingDetails_BookingId ON dbo.BookingDetails(BookingId);
GO

/* Optional starter data (password hashes are placeholders; replace before use). */
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, Role)
VALUES
(N'admin', N'REPLACE_WITH_PASSWORD_HASH', N'System Administrator', N'admin@example.com', 'Admin'),
(N'staff', N'REPLACE_WITH_PASSWORD_HASH', N'Cinema Staff', N'staff@example.com', 'Staff'),
(N'customer', N'REPLACE_WITH_PASSWORD_HASH', N'Sample Customer', N'customer@example.com', 'Customer');
GO

INSERT INTO dbo.Movies (Title, Description, DurationMinutes, Genre, AgeRating, ReleaseDate, Language)
VALUES
(N'Example Movie', N'Replace this sample with a real movie.', 120, N'Drama', N'T13', '2026-01-01', N'Vietnamese');
GO

INSERT INTO dbo.Rooms (RoomName, RoomType)
VALUES (N'Room 1', N'Standard');
GO

/* Example seat layout: rows A-E, seats 1-8. */
;WITH SeatRows AS (
    SELECT v.SeatRow
    FROM (VALUES (N'A'), (N'B'), (N'C'), (N'D'), (N'E')) v(SeatRow)
),
SeatNumbers AS (
    SELECT v.SeatNumber
    FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8)) v(SeatNumber)
)
INSERT INTO dbo.Seats (RoomId, SeatRow, SeatNumber, SeatType)
SELECT r.RoomId, sr.SeatRow, sn.SeatNumber, 'Standard'
FROM dbo.Rooms r
CROSS JOIN SeatRows sr
CROSS JOIN SeatNumbers sn
WHERE r.RoomName = N'Room 1';
GO

/* Example showtime. Change the date/time and price for your test environment. */
INSERT INTO dbo.Showtimes (MovieId, RoomId, StartTime, EndTime, TicketPrice)
SELECT m.MovieId, r.RoomId, '2026-12-01T18:00:00', '2026-12-01T20:00:00', 75000
FROM dbo.Movies m
CROSS JOIN dbo.Rooms r
WHERE m.Title = N'Example Movie' AND r.RoomName = N'Room 1';
GO

/* Relationship summary:
   Users       1-N Bookings (CustomerId)
   Users       0/1-N Bookings (ConfirmedByUserId)
   Movies      1-N Showtimes
   Rooms       1-N Showtimes
   Rooms       1-N Seats
   Showtimes   1-N Bookings
   Bookings    1-N BookingDetails
   Seats       1-N BookingDetails over time/showtimes and cancelled history
   Users       0/1-N BookingDetails (CheckedInByUserId)
*/
