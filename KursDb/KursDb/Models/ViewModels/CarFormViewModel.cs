using Microsoft.AspNetCore.Http; // Для IFormFile
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KursDb.Models.ViewModels
{
    public class CarFormViewModel
    {
        public int? CarId { get; set; } // Для редагування автомобіля
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; } // Зв'язок із категорією

        [ValidateNever] // Ігноруємо це поле у валідації
        public IFormFile ImageFile { get; set; }

        [ValidateNever] // Ігноруємо це поле у валідації
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}
