using KursDb.Context;
using Microsoft.EntityFrameworkCore;

public interface ICarService
{
    List<CarDto> GetAvailableCars(string category, decimal? minPrice, decimal? maxPrice, int? year);
    CarDto? GetCarById(int id);
}

public class CarService : ICarService
{
    private readonly ApplicationDbContext _context;

    public CarService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<CarDto> GetAvailableCars(string category, decimal? minPrice, decimal? maxPrice, int? year)
    {
        var query = _context.Cars
            .Include(c => c.Category)
            .Where(c => c.Status == CarStatus.Available.ToString())
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(c => c.Category != null && c.Category.Name == category);

        if (minPrice.HasValue)
            query = query.Where(c => c.PricePerDay >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(c => c.PricePerDay <= maxPrice.Value);

        if (year.HasValue)
            query = query.Where(c => c.Year == year.Value);

        return query.Select(c => new CarDto
        {
            Id = c.CarId,
            Name = c.Name,
            PricePerDay = c.PricePerDay,
            Year = c.Year,
            Category = c.Category.Name
        }).ToList();
    }

    public CarDto? GetCarById(int id)
    {
        var car = _context.Cars.Include(c => c.Category).FirstOrDefault(c => c.CarId == id);
        return car == null ? null : new CarDto
        {
            Id = car.CarId,
            Name = car.Name,
            Category = car.Category.Name,
            PricePerDay = car.PricePerDay
        };
    }
}
