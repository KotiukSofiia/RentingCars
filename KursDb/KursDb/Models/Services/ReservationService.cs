using KursDb.Context;
using KursDb.Models;
using Microsoft.EntityFrameworkCore;

public interface IReservationService
{
    bool IsDateAvailable(int carId, DateTime startDate, DateTime endDate);
    ReservationDto BookCar(int carId, string userId, DateTime startDate, DateTime endDate);
    List<ReservationDto> GetUserReservations(string userId);
}

public class ReservationService : IReservationService
{
    private readonly ApplicationDbContext _context;

    public ReservationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public bool IsDateAvailable(int carId, DateTime startDate, DateTime endDate)
    {
        return !_context.Reservations.Any(r =>
            r.CarId == carId && r.Status == CarStatus.Confirmed.ToString() &&
            ((startDate >= r.StartDate && startDate < r.EndDate) ||
             (endDate > r.StartDate && endDate <= r.EndDate) ||
             (startDate <= r.StartDate && endDate >= r.EndDate)));
    }

    public ReservationDto BookCar(int carId, string userId, DateTime startDate, DateTime endDate)
    {
        var car = _context.Cars.FirstOrDefault(c => c.CarId == carId && c.Status == CarStatus.Available.ToString());
        if (car == null) return null;

        if (!IsDateAvailable(carId, startDate, endDate))
            throw new InvalidOperationException("Car is already booked for the selected dates.");

        var reservation = new Reservation
        {
            CarId = carId,
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            TotalPrice = (decimal)((endDate - startDate).TotalDays) * car.PricePerDay,
            Status = CarStatus.Pending.ToString()
        };

        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        return new ReservationDto
        {
            CarId = carId,
            StartDate = startDate,
            EndDate = endDate,
            TotalPrice = reservation.TotalPrice
        };
    }

    public List<ReservationDto> GetUserReservations(string userId)
    {
        return _context.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.Car)
            .Select(r => new ReservationDto
            {
                CarId = r.CarId,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TotalPrice = r.TotalPrice
            }).ToList();
    }
}
