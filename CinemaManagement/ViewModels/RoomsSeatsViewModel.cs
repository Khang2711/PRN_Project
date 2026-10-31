using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace CinemaManagement.ViewModels;

public class RoomsSeatsViewModel : BaseViewModel
{
    public ObservableCollection<Room> Rooms { get; }

    public ObservableCollection<Seat> Seats { get; }

    private Room? _selectedRoom;

    public Room? SelectedRoom
    {
        get
        {
            return _selectedRoom;
        }
        set
        {
            if (_selectedRoom == value)
            {
                return;
            }

            _selectedRoom = value;
            OnPropertyChanged();

            LoadSeats();
        }
    }

    public RoomsSeatsViewModel()
    {
        Rooms = new ObservableCollection<Room>();
        Seats = new ObservableCollection<Seat>();

        try
        {
            using var db = new CinemaManagementDbContext();

            // Lay danh sach phong va ghe tu database
            var data = (
                from r in db.Rooms
                    .Include("Seats")
                    .AsNoTracking()
                where r.IsActive
                orderby r.RoomId
                select r
            ).ToList();

            Rooms.Clear();

            foreach (Room room in data)
            {
                Rooms.Add(room);
            }

            if (Rooms.Count > 0)
            {
                SelectedRoom = Rooms[0];
            }
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

        // Lay danh sach ghe cua phong duoc chon
        var data = (
            from s in SelectedRoom.Seats
            where s.IsActive
            orderby s.SeatRow, s.SeatNumber
            select s
        ).ToList();

        foreach (Seat seat in data)
        {
            Seats.Add(seat);
        }
    }
}
