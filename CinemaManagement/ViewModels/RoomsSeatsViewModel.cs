using CinemaManagement.Models;
using System.Collections.ObjectModel;

namespace CinemaManagement.ViewModels;

public class RoomsSeatsViewModel : BaseViewModel
{
    public ObservableCollection<Room> Rooms { get; }

    public ObservableCollection<Seat> Seats { get; }

    private readonly List<Seat> _allSeats;

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
        _allSeats = new List<Seat>();

        Rooms.Add(new Room
        {
            RoomId = 1,
            RoomName = "Phòng 1",
            TotalSeats = 80
        });

        Rooms.Add(new Room
        {
            RoomId = 2,
            RoomName = "Phòng 2",
            TotalSeats = 100
        });

        Rooms.Add(new Room
        {
            RoomId = 3,
            RoomName = "Phòng 3",
            TotalSeats = 120
        });

        CreateSampleSeats(1, 8, 10);
        CreateSampleSeats(2, 10, 10);
        CreateSampleSeats(3, 12, 10);

        SelectedRoom = Rooms[0];
    }

    private void CreateSampleSeats(
        int roomId,
        int totalRows,
        int seatsPerRow)
    {
        for (int row = 0; row < totalRows; row++)
        {
            char rowLabel = (char)('A' + row);

            for (int number = 1; number <= seatsPerRow; number++)
            {
                Seat seat = new Seat
                {
                    SeatId = _allSeats.Count + 1,
                    RoomId = roomId,
                    SeatLabel = rowLabel.ToString() + number,
                    IsActive = true
                };

                _allSeats.Add(seat);
            }
        }
    }

    private void LoadSeats()
    {
        Seats.Clear();

        if (SelectedRoom == null)
        {
            return;
        }

        foreach (Seat seat in _allSeats)
        {
            if (seat.RoomId == SelectedRoom.RoomId)
            {
                Seats.Add(seat);
            }
        }
    }
}
