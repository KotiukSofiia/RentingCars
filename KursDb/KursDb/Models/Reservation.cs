using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace KursDb.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public string UserId { get; set; } // ID користувача (FK із AspNetUsers)
        public int CarId { get; set; }     // ID автомобіля (FK із Cars)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Очікує підтвердження"; // Статус бронювання

        public Car Car { get; set; } // Навігаційна властивість
        public IdentityUser User { get; set; } // Навігаційна властивість
    }

}
