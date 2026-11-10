using CinemaManagement.Models;
using CinemaManagement.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CinemaManagement.ViewModels;

public class RoomsSeatsViewModel : BaseViewModel
{
    public ObservableCollection<Room> Rooms { get; }
    public ObservableCollection<Seat> Seats { get; }

    public ICommand AddRoomCommand { get; }
    public ICommand EditRoomCommand { get; }
    public ICommand DeleteRoomCommand { get; }
    public ICommand RestoreRoomCommand { get; }

    public ICommand AddSeatCommand { get; }
    public ICommand EditSeatCommand { get; }
    public ICommand DeleteSeatCommand { get; }

    private bool _showDeletedRooms;

    public bool ShowDeletedRooms
    {
        get { return _showDeletedRooms; }
        set
        {
            if (_showDeletedRooms == value) return;

            _showDeletedRooms = value;
            OnPropertyChanged();
            // Reload rooms when the deleted-room filter changes.
            LoadRooms(SelectedRoom?.RoomId);
        }
    }

    private Room? _selectedRoom;

    public Room? SelectedRoom
    {
        get { return _selectedRoom; }
        set
        {
            if (_selectedRoom == value) return;

            _selectedRoom = value;
            OnPropertyChanged();

            if (SelectedRoom == null)
            {
                RoomNameInput = "";
                RoomTypeInput = "Standard";
                RoomActiveInput = true;
                SelectedSeat = null;
                Seats.Clear();
                return;
            }

            RoomNameInput = SelectedRoom.RoomName;
            RoomTypeInput = SelectedRoom.RoomType ?? "Standard";
            RoomActiveInput = SelectedRoom.IsActive;

            SelectedSeat = null;
            // Refresh the seat list when the selected room changes.
            LoadSeats();
        }
    }

    private Seat? _selectedSeat;

    public Seat? SelectedSeat
    {
        get { return _selectedSeat; }
        set
        {
            if (_selectedSeat == value) return;

            _selectedSeat = value;
            OnPropertyChanged();

            if (SelectedSeat == null)
            {
                SeatRowInput = "";
                SeatNumberInput = "";
                SeatTypeInput = "Standard";
                SeatActiveInput = true;
                return;
            }

            SeatRowInput = SelectedSeat.SeatRow;
            SeatNumberInput = SelectedSeat.SeatNumber.ToString();
            SeatTypeInput = SelectedSeat.SeatType;
            SeatActiveInput = SelectedSeat.IsActive;
        }
    }

    private string _roomNameInput = "";

    public string RoomNameInput
    {
        get { return _roomNameInput; }
        set
        {
            _roomNameInput = value;
            OnPropertyChanged();
        }
    }

    private string _roomTypeInput = "Standard";

    public string RoomTypeInput
    {
        get { return _roomTypeInput; }
        set
        {
            _roomTypeInput = value;
            OnPropertyChanged();
        }
    }

    private bool _roomActiveInput = true;

    public bool RoomActiveInput
    {
        get { return _roomActiveInput; }
        set
        {
            _roomActiveInput = value;
            OnPropertyChanged();
        }
    }

    private string _seatRowInput = "";

    public string SeatRowInput
    {
        get { return _seatRowInput; }
        set
        {
            _seatRowInput = value;
            OnPropertyChanged();
        }
    }

    private string _seatNumberInput = "";

    public string SeatNumberInput
    {
        get { return _seatNumberInput; }
        set
        {
            _seatNumberInput = value;
            OnPropertyChanged();
        }
    }

    private string _seatTypeInput = "Standard";

    public string SeatTypeInput
    {
        get { return _seatTypeInput; }
        set
        {
            _seatTypeInput = value;
            OnPropertyChanged();
        }
    }

    private bool _seatActiveInput = true;

    public bool SeatActiveInput
    {
        get { return _seatActiveInput; }
        set
        {
            _seatActiveInput = value;
            OnPropertyChanged();
        }
    }

    public RoomsSeatsViewModel()
    {
        // Initialize the lists, connect commands, and load the first room.
        Rooms = new ObservableCollection<Room>();
        Seats = new ObservableCollection<Seat>();

        AddRoomCommand = new RelayCommand(_ => AddRoom());
        EditRoomCommand = new RelayCommand(_ => EditRoom());
        DeleteRoomCommand = new RelayCommand(_ => DeleteRoom());
        RestoreRoomCommand = new RelayCommand(_ => RestoreRoom());

        AddSeatCommand = new RelayCommand(_ => AddSeat());
        EditSeatCommand = new RelayCommand(_ => EditSeat());
        DeleteSeatCommand = new RelayCommand(_ => DeleteSeat());

        LoadRooms();
    }

    private void LoadRooms(int? selectedRoomId = null)
    {
        try
        {
            using var db = new CinemaManagementDbContext();

            var data = db.Rooms
                // Load active seats with each room for the seat list.
                .Include(r => r.Seats.Where(s => s.IsActive))
                // These records are only displayed, so change tracking is not needed.
                .AsNoTracking()
                .Where(r => ShowDeletedRooms || r.IsActive)
                .OrderBy(r => r.RoomId)
                .ToList();

            Rooms.Clear();

            foreach (Room room in data)
            {
                Rooms.Add(room);
            }

            // Keep the previous selection after reloading the room list.
            Room? roomToSelect = null;

            if (selectedRoomId.HasValue)
            {
                roomToSelect = Rooms.FirstOrDefault(
                    r => r.RoomId == selectedRoomId.Value
                );
            }

            if (roomToSelect == null && Rooms.Count > 0)
            {
                roomToSelect = Rooms[0];
            }

            SelectedRoom = roomToSelect;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot load rooms from database!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void LoadSeats()
    {
        Seats.Clear();

        if (SelectedRoom == null)
        {
            return;
        }

        // Show active seats in row and seat-number order.
        var data = SelectedRoom.Seats
            .Where(s => s.IsActive)
            .OrderBy(s => s.SeatRow)
            .ThenBy(s => s.SeatNumber)
            .ToList();

        foreach (Seat seat in data)
        {
            Seats.Add(seat);
        }
    }

    // A room with a current or upcoming showtime must remain active.
    private static bool HasCurrentOrUpcomingShowtimes(
        CinemaManagementDbContext db,
        int roomId)
    {
        DateTime now = DateTime.Now;

        return db.Showtimes.Any(showtime =>
            showtime.RoomId == roomId &&
            showtime.IsActive &&
            showtime.EndTime >= now);
    }

    // Protect seats that are still linked to valid booking records.
    private static bool HasNonCancelledBooking(
        CinemaManagementDbContext db,
        int seatId)
    {
        return db.BookingDetails.Any(detail =>
            detail.SeatId == seatId &&
            !detail.IsCancelled &&
            !detail.Booking.IsCancelled &&
            detail.Booking.Status != "Cancelled");
    }

    private void AddRoom()
    {
        if (string.IsNullOrWhiteSpace(RoomNameInput))
        {
            MessageBox.Show("Please enter room name!");
            return;
        }

        if (RoomNameInput.Trim().Length > 50)
        {
            MessageBox.Show("Room name cannot exceed 50 characters!");
            return;
        }

        if (string.IsNullOrWhiteSpace(RoomTypeInput))
        {
            MessageBox.Show("Please enter room type!");
            return;
        }

        if (RoomTypeInput.Trim().Length > 50)
        {
            MessageBox.Show("Room type cannot exceed 50 characters!");
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            string roomName = RoomNameInput.Trim();
            // Room names must be unique.
            if (db.Rooms.Any(r => r.RoomName == roomName))
            {
                MessageBox.Show(
                    "A room with this name already exists!",
                    "Duplicate Room",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Room room = new Room
            {
                RoomName = RoomNameInput.Trim(),
                RoomType = RoomTypeInput.Trim(),
                IsActive = RoomActiveInput
            };

            db.Rooms.Add(room);
            db.SaveChanges();

            int roomId = room.RoomId;

            MessageBox.Show("Room added successfully!");

            LoadRooms(roomId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot add room!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void EditRoom()
    {
        if (SelectedRoom == null)
        {
            MessageBox.Show("Please select a room!");
            return;
        }

        if (string.IsNullOrWhiteSpace(RoomNameInput))
        {
            MessageBox.Show("Please enter room name!");
            return;
        }

        if (RoomNameInput.Trim().Length > 50)
        {
            MessageBox.Show("Room name cannot exceed 50 characters!");
            return;
        }

        if (string.IsNullOrWhiteSpace(RoomTypeInput))
        {
            MessageBox.Show("Please enter room type!");
            return;
        }

        if (RoomTypeInput.Trim().Length > 50)
        {
            MessageBox.Show("Room type cannot exceed 50 characters!");
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            string roomName = RoomNameInput.Trim();
            // Exclude the current room when checking for duplicate names.
            if (db.Rooms.Any(r =>
                r.RoomName == roomName &&
                r.RoomId != SelectedRoom.RoomId))
            {
                MessageBox.Show(
                    "A room with this name already exists!",
                    "Duplicate Room",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Room? room = db.Rooms.FirstOrDefault(
                r => r.RoomId == SelectedRoom.RoomId
            );

            if (room == null)
            {
                MessageBox.Show("Room not found!");
                return;
            }

            // Do not deactivate a room while it has a current or upcoming showtime.
            if (room.IsActive && !RoomActiveInput &&
                HasCurrentOrUpcomingShowtimes(db, room.RoomId))
            {
                MessageBox.Show(
                    "This room cannot be deactivated because it has a current or upcoming showtime.",
                    "Room In Use",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            room.RoomName = roomName;
            room.RoomType = RoomTypeInput.Trim();
            room.IsActive = RoomActiveInput;

            db.SaveChanges();

            int roomId = room.RoomId;

            MessageBox.Show("Room updated successfully!");

            LoadRooms(roomId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot update room!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void DeleteRoom()
    {
        if (SelectedRoom == null)
        {
            MessageBox.Show("Please select a room!");
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to delete this room?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            Room? room = db.Rooms.FirstOrDefault(
                r => r.RoomId == SelectedRoom.RoomId
            );

            if (room == null)
            {
                MessageBox.Show("Room not found!");
                return;
            }

            if (!room.IsActive)
            {
                MessageBox.Show("This room is already deleted. Use Restore to activate it again!");
                return;
            }

            if (HasCurrentOrUpcomingShowtimes(db, room.RoomId))
            {
                MessageBox.Show(
                    "This room cannot be deleted because it has a current or upcoming showtime. " +
                    "Deactivate or reschedule those showtimes first.",
                    "Room In Use",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Soft delete: keep the room record for existing references.
            room.IsActive = false;

            db.SaveChanges();

            MessageBox.Show("Room deleted successfully!");

            LoadRooms();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot delete room!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void RestoreRoom()
    {
        if (SelectedRoom == null)
        {
            MessageBox.Show("Please select a room!");
            return;
        }

        if (SelectedRoom.IsActive)
        {
            MessageBox.Show("The selected room is already active!");
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            "Do you want to restore this room?",
            "Restore Room",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            Room? room = db.Rooms.FirstOrDefault(
                r => r.RoomId == SelectedRoom.RoomId);

            if (room == null)
            {
                MessageBox.Show("Room not found!");
                return;
            }

            if (room.IsActive)
            {
                MessageBox.Show("This room is already active!");
                LoadRooms(room.RoomId);
                return;
            }

            // Restore the existing room instead of creating a new record.
            room.IsActive = true;
            db.SaveChanges();

            MessageBox.Show("Room restored successfully!");
            LoadRooms(room.RoomId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot restore room!\n\n" + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void AddSeat()
    {
        if (SelectedRoom == null)
        {
            MessageBox.Show("Please select a room first!");
            return;
        }

        if (!SelectedRoom.IsActive)
        {
            MessageBox.Show("Restore this room before managing its seats!");
            return;
        }

        if (string.IsNullOrWhiteSpace(SeatRowInput))
        {
            MessageBox.Show("Please enter seat row!");
            return;
        }

        if (SeatRowInput.Trim().Length > 5)
        {
            MessageBox.Show("Seat row cannot exceed 5 characters!");
            return;
        }

        if (!int.TryParse(SeatNumberInput, out int seatNumber)
            || seatNumber <= 0)
        {
            MessageBox.Show("Seat number must be a positive number!");
            return;
        }

        if (string.IsNullOrWhiteSpace(SeatTypeInput))
        {
            MessageBox.Show("Please select seat type!");
            return;
        }

        if (SeatTypeInput.Trim().Length > 20)
        {
            MessageBox.Show("Seat type cannot exceed 20 characters!");
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            int roomId = SelectedRoom.RoomId;
            string seatRow = SeatRowInput.Trim().ToUpper();

            // A seat position must be unique within its room.
            Seat? existingSeat = db.Seats.FirstOrDefault(s =>
                s.RoomId == roomId &&
                s.SeatRow == seatRow &&
                s.SeatNumber == seatNumber
            );

            if (existingSeat != null)
            {
                if (existingSeat.IsActive)
                {
                    MessageBox.Show(
                        "This seat position already exists in this room!",
                        "Duplicate Seat",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }

                MessageBoxResult result = MessageBox.Show(
                    "This seat was deleted before. Do you want to restore it?",
                    "Restore Seat",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // Restore the old seat record to avoid a duplicate position.
                existingSeat.IsActive = true;

                db.SaveChanges();

                MessageBox.Show("Seat restored successfully!");

                LoadRooms(roomId);
                return;
            }

            Seat seat = new Seat
            {
                RoomId = roomId,
                SeatRow = seatRow,
                SeatNumber = seatNumber,
                SeatType = SeatTypeInput.Trim(),
                IsActive = SeatActiveInput
            };

            db.Seats.Add(seat);
            db.SaveChanges();

            MessageBox.Show("Seat added successfully!");

            LoadRooms(roomId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot add seat!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void EditSeat()
    {
        if (SelectedRoom == null)
        {
            MessageBox.Show("Please select a room first!");
            return;
        }

        if (SelectedSeat == null)
        {
            MessageBox.Show("Please select a seat!");
            return;
        }

        if (!SelectedRoom.IsActive)
        {
            MessageBox.Show("Restore this room before managing its seats!");
            return;
        }

        if (string.IsNullOrWhiteSpace(SeatRowInput))
        {
            MessageBox.Show("Please enter seat row!");
            return;
        }

        if (SeatRowInput.Trim().Length > 5)
        {
            MessageBox.Show("Seat row cannot exceed 5 characters!");
            return;
        }

        if (!int.TryParse(SeatNumberInput, out int seatNumber)
            || seatNumber <= 0)
        {
            MessageBox.Show("Seat number must be a positive number!");
            return;
        }

        if (string.IsNullOrWhiteSpace(SeatTypeInput))
        {
            MessageBox.Show("Please select seat type!");
            return;
        }

        if (SeatTypeInput.Trim().Length > 20)
        {
            MessageBox.Show("Seat type cannot exceed 20 characters!");
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            Seat? seat = db.Seats.FirstOrDefault(
                s => s.SeatId == SelectedSeat.SeatId
            );

            if (seat == null)
            {
                MessageBox.Show("Seat not found!");
                return;
            }

            // Do not change a seat that is still used by a non-cancelled booking.
            if (HasNonCancelledBooking(db, seat.SeatId))
            {
                MessageBox.Show(
                    "This seat is linked to a non-cancelled booking and cannot be edited. " +
                    "This protects existing ticket and booking records.",
                    "Seat Already Booked",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string newSeatRow = SeatRowInput.Trim().ToUpper();

            // Exclude the current seat when checking its new position.
            bool isDuplicate = db.Seats.Any(s =>
                s.RoomId == SelectedRoom.RoomId &&
                s.SeatRow == newSeatRow &&
                s.SeatNumber == seatNumber &&
                s.SeatId != seat.SeatId
            );

            if (isDuplicate)
            {
                MessageBox.Show(
                    "This seat position already exists in this room!\n" +
                    "Please choose another row or seat number.",
                    "Duplicate Seat",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }

            seat.SeatRow = newSeatRow;
            seat.SeatNumber = seatNumber;
            seat.SeatType = SeatTypeInput.Trim();
            seat.IsActive = SeatActiveInput;

            db.SaveChanges();

            int roomId = SelectedRoom.RoomId;

            MessageBox.Show("Seat updated successfully!");

            LoadRooms(roomId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot update seat!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void DeleteSeat()
    {
        if (SelectedRoom == null)
        {
            MessageBox.Show("Please select a room first!");
            return;
        }

        if (SelectedSeat == null)
        {
            MessageBox.Show("Please select a seat!");
            return;
        }

        if (!SelectedRoom.IsActive)
        {
            MessageBox.Show("Restore this room before managing its seats!");
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to delete this seat?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            using var db = new CinemaManagementDbContext();

            Seat? seat = db.Seats.FirstOrDefault(
                s => s.SeatId == SelectedSeat.SeatId
            );

            if (seat == null)
            {
                MessageBox.Show("Seat not found!");
                return;
            }

            // Keep seats that are part of valid bookings to protect booking history.
            if (HasNonCancelledBooking(db, seat.SeatId))
            {
                MessageBox.Show(
                    "This seat is linked to a non-cancelled booking and cannot be deleted. " +
                    "This protects existing ticket and booking records.",
                    "Seat Already Booked",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Soft delete: preserve the seat record for booking history.
            seat.IsActive = false;

            db.SaveChanges();

            int roomId = SelectedRoom.RoomId;

            MessageBox.Show("Seat deleted successfully!");

            LoadRooms(roomId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Cannot delete seat!\n\n"
                + ex.GetBaseException().Message,
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}