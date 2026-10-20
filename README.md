# Cinema Management - khung WPF MVVM cơ bản (PRN212)

Bản này dành cho nhóm mới học WPF. Mỗi file đều có **mã khởi tạo tối thiểu** (khai báo `class`, `namespace`, `x:Class`, `InitializeComponent()`...), **không còn file trống**.

- **Đã có:** project WPF .NET 8, `App`, `MainWindow`, 7 cặp `View`/`ViewModel`, 3 lớp `Model`, `BaseViewModel`, `RelayCommand`.
- **Chưa có:** đăng nhập, CRUD, đặt vé, thanh toán, kết nối SQL Server hay chức năng nghiệp vụ.
- `Data/CinemaDbContext.cs` **chỉ là khung class**, chưa phải EF Core `DbContext` thật và chưa cần cài NuGet.
- Khi chạy, ứng dụng chỉ mở cửa sổ **Cinema Management - PRN212**. Các màn hình trong `Views` sẽ được nhóm kết nối sau.

## Mở trong Visual Studio

1. Cài **Visual Studio 2022** trên **Windows**, chọn workload **.NET desktop development** và bộ công cụ **.NET 8**.
2. Giải nén, mở `CinemaManagement.sln`.
3. Chọn `CinemaManagement` làm Startup Project nếu cần, nhấn **F5**.
4. Để làm phần được giao, sửa cặp file trong `Views/` và `ViewModels/` tương ứng. File `.xaml.cs` chỉ giữ `InitializeComponent()` để phù hợp MVVM.

## Cấu trúc

```text
CinemaManagement.sln
CinemaManagement/
├── CinemaManagement.csproj
├── App.xaml + App.xaml.cs
├── MainWindow.xaml + MainWindow.xaml.cs
├── Models/        (Movie, User, Booking)
├── Views/         (7 UserControl .xaml + .xaml.cs)
├── ViewModels/    (BaseViewModel + 7 ViewModel)
├── Helpers/       (RelayCommand.cs)
└── Data/          (CinemaDbContext.cs - chờ học EF Core)
```

## Phân công theo BACKLOG_PRN.xlsm

| Thành viên | View và ViewModel | Chức năng |
|---|---|---|
| Nguyễn Hoàng Gia Kiệt | BrowseMovies | F01-F05: Xem phim |
| Lê Hoàng Khang | Account | F06-F10: Tài khoản |
| Dương Di Hào | ManageMovies | F11-F14 và F39: Quản lý phim, thống kê |
| Lý Chi Vy | RoomsSeats | F15-F22: Phòng và ghế |
| Trần Hoàng | Showtimes | F23-F27: Suất chiếu, giá vé |
| Phạm Thế Thịnh | Booking | F28-F32: Đặt vé |
| Nguyễn Thành Đạt | Payments | F33-F38: Thanh toán, vé và người dùng |

Mỗi bạn có **1 View + 1 ViewModel** và **1 file code-behind `.xaml.cs`** đi kèm, không phải mỗi chức năng là một file.

## Đẩy lên GitHub

Mở Terminal tại thư mục chứa `.sln`:

```bash
git init
git add .
git commit -m "Initial WPF MVVM project"
git branch -M main
git remote add origin <URL_REPOSITORY_GITHUB>
git push -u origin main
```

Thay `<URL_REPOSITORY_GITHUB>` bằng đường dẫn repo của nhóm. Mỗi bạn nên tạo branch riêng và mở Pull Request vào `main`. File `.gitignore` đã loại bỏ `bin/`, `obj/`, `.vs/`.

**Lưu ý tài liệu:** Bảng phân công lấy từ file nhóm đã cung cấp. Report 1/3 đang mô tả Web Application; nhóm làm WPF Desktop cần cập nhật báo cáo tương ứng.
# PRN_Project
