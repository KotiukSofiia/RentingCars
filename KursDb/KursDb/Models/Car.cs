using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KursDb.Models
{
    public class Car
    {
        public int CarId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal PricePerDay { get; set; }
        public string Status { get; set; } = "Доступно";
        public double AverageRating { get; set; }
        public string? Description { get; set; } 
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<CarRating> CarRatings { get; set; }
        public int CategoryId { get; set; }
        [ValidateNever] 
        public CarCategory Category { get; set; }
    }

}
