USE CinemaManagementDB;
GO

-- ==========================================
-- 1. XÓA DỮ LIỆU CŨ (Nếu có) ĐỂ TRÁNH TRÙNG LẶP
-- ==========================================
DELETE FROM dbo.BookingDetails;
DELETE FROM dbo.Bookings;
DELETE FROM dbo.Showtimes;
DELETE FROM dbo.Seats;
DELETE FROM dbo.Rooms;
DELETE FROM dbo.Movies;
DELETE FROM dbo.Users;
GO

-- ==========================================
-- 2. ĐỔ DỮ LIỆU USERS (TÀI KHOẢN)
-- ==========================================
-- Mật khẩu tạm thời đang để chuỗi hash giả. Nhóm tự thay đổi thuật toán mã hóa sau.
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, Phone, Role) VALUES
(N'admin_dat', N'123456_hash', N'Nguyễn Thành Đạt', N'datnt@cinema.com', '0901234567', 'Admin'),
(N'staff_thinh', N'123456_hash', N'Phạm Thế Thịnh', N'thinhpt@cinema.com', '0912345678', 'Staff'),
(N'khang_user', N'123456_hash', N'Lê Hoàng Khang', N'khanglh@gmail.com', '0334155209', 'Customer'),
(N'kiet_user', N'123456_hash', N'Nguyễn Hoàng Gia Kiệt', N'kietnh@gmail.com', '0704847434', 'Customer'),
(N'hao_user', N'123456_hash', N'Dương Di Hào', N'haodd@gmail.com', '0933333333', 'Customer');
GO

-- ==========================================
-- 3. ĐỔ DỮ LIỆU MOVIES (PHIM)
-- ==========================================
INSERT INTO dbo.Movies (Title, Description, DurationMinutes, Genre, AgeRating, ReleaseDate, Language, PosterUrl) VALUES
(N'Lật Mặt 10: Dấu Vết', N'Phim hành động kịch tính của đạo diễn Lý Hải.', 120, N'Hành động', N'T18', '2026-10-25', N'Tiếng Việt', N'latmat10.jpg'),
(N'Venom 3: The Last Dance', N'Phần cuối cùng của gã cộng sinh Venom.', 135, N'Hành động/Viễn tưởng', N'T16', '2026-10-31', N'Tiếng Anh', N'venom3.jpg'),
(N'Mai 2: Hạnh Phúc Bỏ Ngỏ', N'Hậu truyện của phim điện ảnh Mai.', 130, N'Tâm lý/Tình cảm', N'T16', '2026-11-20', N'Tiếng Việt', N'mai2.jpg'),
(N'Doraemon: Nobita và Đảo Châu Báu 2', N'Hoạt hình gia đình vui nhộn.', 105, N'Hoạt hình', N'P', '2026-12-24', N'Lồng tiếng Việt', N'doraemon.jpg'),
(N'Avatar 3: Hạt Giống Lửa', N'Siêu bom tấn hình ảnh của James Cameron.', 160, N'Viễn tưởng/Phiêu lưu', N'T13', '2027-01-20', N'Tiếng Anh', N'avatar3.jpg');
GO

-- ==========================================
-- 4. ĐỔ DỮ LIỆU ROOMS (PHÒNG CHIẾU)
-- ==========================================
INSERT INTO dbo.Rooms (RoomName, RoomType) VALUES 
(N'Cinema 01', N'Standard'),
(N'Cinema 02', N'IMAX'),
(N'Cinema 03', N'Couple');
GO

-- ==========================================
-- 5. ĐỔ DỮ LIỆU SEATS (GHẾ) CHO TẤT CẢ CÁC PHÒNG
-- ==========================================
-- Dùng CTE để tự động tạo 50 ghế (A-E, 1-10) cho mỗi phòng
;WITH SeatRows AS (
    SELECT v.SeatRow FROM (VALUES (N'A'), (N'B'), (N'C'), (N'D'), (N'E')) v(SeatRow)
),
SeatNumbers AS (
    SELECT v.SeatNumber FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) v(SeatNumber)
)
INSERT INTO dbo.Seats (RoomId, SeatRow, SeatNumber, SeatType)
SELECT r.RoomId, sr.SeatRow, sn.SeatNumber, 
       CASE WHEN r.RoomType = N'Couple' THEN 'Couple'
            WHEN sr.SeatRow IN (N'D', N'E') THEN 'VIP' 
            ELSE 'Standard' END
FROM dbo.Rooms r
CROSS JOIN SeatRows sr
CROSS JOIN SeatNumbers sn;
GO

-- ==========================================
-- 6. ĐỔ DỮ LIỆU SHOWTIMES (LỊCH CHIẾU - RẤT NHIỀU VÀ XA NHAU)
-- ==========================================
-- Khai báo biến để lấy ID tự động nhằm tránh lỗi Identity
DECLARE @M1 INT = (SELECT MovieId FROM dbo.Movies WHERE Title LIKE N'Lật Mặt 10%');
DECLARE @M2 INT = (SELECT MovieId FROM dbo.Movies WHERE Title LIKE N'Venom 3%');
DECLARE @M3 INT = (SELECT MovieId FROM dbo.Movies WHERE Title LIKE N'Mai 2%');
DECLARE @M4 INT = (SELECT MovieId FROM dbo.Movies WHERE Title LIKE N'Doraemon%');
DECLARE @M5 INT = (SELECT MovieId FROM dbo.Movies WHERE Title LIKE N'Avatar 3%');

DECLARE @R1 INT = (SELECT RoomId FROM dbo.Rooms WHERE RoomName = N'Cinema 01');
DECLARE @R2 INT = (SELECT RoomId FROM dbo.Rooms WHERE RoomName = N'Cinema 02');
DECLARE @R3 INT = (SELECT RoomId FROM dbo.Rooms WHERE RoomName = N'Cinema 03');

INSERT INTO dbo.Showtimes (MovieId, RoomId, StartTime, EndTime, TicketPrice) VALUES
-- THÁNG 10/2026 (Gần thời điểm hiện tại)
(@M1, @R1, '2026-10-25T09:00:00', '2026-10-25T11:00:00', 65000),
(@M1, @R2, '2026-10-25T19:30:00', '2026-10-25T21:30:00', 85000),
(@M2, @R2, '2026-10-31T20:00:00', '2026-10-31T22:15:00', 90000), -- Halloween

-- THÁNG 11/2026 
(@M2, @R1, '2026-11-10T14:00:00', '2026-11-10T16:15:00', 70000),
(@M3, @R3, '2026-11-20T19:00:00', '2026-11-20T21:10:00', 120000), -- 20/11, Phòng Couple
(@M3, @R1, '2026-11-25T18:00:00', '2026-11-25T20:10:00', 75000),

-- THÁNG 12/2026 (Mùa Giáng Sinh)
(@M3, @R2, '2026-12-15T20:00:00', '2026-12-15T22:10:00', 85000),
(@M4, @R1, '2026-12-24T09:00:00', '2026-12-24T10:45:00', 50000), -- Sáng Noel cho thiếu nhi
(@M4, @R2, '2026-12-25T14:30:00', '2026-12-25T16:15:00', 60000), 
(@M2, @R3, '2026-12-31T22:00:00', '2027-01-01T00:15:00', 150000), -- Đón giao thừa Dương Lịch

-- THÁNG 1/2027 & THÁNG 2/2027 (Tết Nguyên Đán)
(@M5, @R2, '2027-01-20T19:00:00', '2027-01-20T21:40:00', 100000), -- Suất công chiếu Avatar 3
(@M5, @R2, '2027-01-25T13:00:00', '2027-01-25T15:40:00', 95000),
(@M5, @R1, '2027-02-06T18:30:00', '2027-02-06T21:10:00', 110000), -- Mùng 1 Tết Nguyên Đán
(@M4, @R1, '2027-02-07T09:30:00', '2027-02-07T11:15:00', 65000);  -- Mùng 2 Tết Nguyên Đán
GO

PRINT N'✅ ĐÃ SEED THÀNH CÔNG DỮ LIỆU TÀI KHOẢN, PHIM, PHÒNG, GHẾ VÀ LỊCH CHIẾU ĐA DẠNG!';