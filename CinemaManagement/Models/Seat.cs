using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CinemaManagement.Models
{
    public class Seat
    {

        public int SeatId { get; set; }
        public int RoomId { get; set; }

        public String SeatLabel { get; set; } = "";

        public bool IsActive { get; set; }
    }
}
