using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KursDb.Models
{
    public class CarRating
    {
        public int CarRatingId { get; set; }
        public int CarId { get; set; } // Зв'язок із автомобілем
        public string UserId { get; set; } // Зв'язок із користувачем
        public int Rating { get; set; } // Оцінка від 1 до 5

        public Car Car { get; set; } // Навігаційна властивість
        public IdentityUser User { get; set; } // Навігаційна властивість
    }

}
