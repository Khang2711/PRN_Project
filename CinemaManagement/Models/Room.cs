using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Models
{
    public class Room
    {

        public int RoomId { get; set; }

        public string RoomName { get; set; } = "";

        public int TotalSeats { get; set; }
    }
}
