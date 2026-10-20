using CinemaManagement.Models;
using System.Collections.ObjectModel;

namespace CinemaManagement.ViewModels;

public class RoomsSeatsViewModel : BaseViewModel
{
  
        public ObservableCollection<Room> Rooms { get; }

        public RoomsSeatsViewModel()
        {
            Rooms = new ObservableCollection<Room>();

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
        }
    }



