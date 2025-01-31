using KursDb.Context;
using KursDb.Models;
using Microsoft.EntityFrameworkCore;

public interface IRatingService
{
    void RateCar(int carId, string userId, int rating);
    double GetAverageRating(int carId);
}

public class RatingService : IRatingService
{
    private readonly ApplicationDbContext _context;

    public RatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void RateCar(int carId, string userId, int rating)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Рейтинг має бути від 1 до 5");

        var existingRating = _context.CarRatings.FirstOrDefault(r => r.CarId == carId && r.UserId == userId);

        if (existingRating != null)
        {
            existingRating.Rating = rating;
        }
        else
        {
            var newRating = new CarRating
            {
                CarId = carId,
                UserId = userId,
                Rating = rating
            };
            _context.CarRatings.Add(newRating);
        }

        _context.SaveChanges();
        UpdateAverageRating(carId);
    }

    public double GetAverageRating(int carId)
    {
        var car = _context.Cars.Include(c => c.CarRatings).FirstOrDefault(c => c.CarId == carId);
        return car != null && car.CarRatings.Any() ? car.CarRatings.Average(r => r.Rating) : 0;
    }

    private void UpdateAverageRating(int carId)
    {
        var car = _context.Cars.Include(c => c.CarRatings).FirstOrDefault(c => c.CarId == carId);
        if (car != null)
        {
            car.AverageRating = car.CarRatings.Any() ? car.CarRatings.Average(r => r.Rating) : 0;
            _context.SaveChanges();
        }
    }
}