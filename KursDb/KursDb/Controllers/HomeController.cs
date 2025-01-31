using KursDb.Context;
using KursDb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace KursDb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cars = _context.Cars.ToList();
            return View(cars);
        }
        public IActionResult Details(int id)
        {
            var car = _context.Cars
        .Include(c => c.Category)
        .FirstOrDefault(c => c.CarId == id);

            if (car == null)
            {
                return NotFound();
            }

            var reservations = _context.Reservations
                .Where(r => r.CarId == id && r.Status == "Підтверджено")
                .Select(r => new ReservationDate
                {
                    StartDate = r.StartDate,
                    EndDate = r.EndDate
                })
                .ToList();

            ViewBag.Reservations = reservations;

            return View(car);
        }

        public IActionResult AvailableCars(string category, decimal? minPrice, decimal? maxPrice, int? year)
        {
            // Завантажуємо всі доступні автомобілі з категоріями
            var cars = _context.Cars
                .Include(c => c.Category) // Завантажуємо навігаційну властивість
                .Where(c => c.Status == "Доступно")
                .AsQueryable();

            // Фільтрація за категорією
            if (!string.IsNullOrEmpty(category))
            {
                cars = cars.Where(c => c.Category != null && c.Category.Name == category);
            }

            // Фільтрація за мінімальною ціною
            if (minPrice.HasValue)
            {
                cars = cars.Where(c => c.PricePerDay >= minPrice.Value);
            }

            // Фільтрація за максимальною ціною
            if (maxPrice.HasValue)
            {
                cars = cars.Where(c => c.PricePerDay <= maxPrice.Value);
            }

            // Фільтрація за роком
            if (year.HasValue)
            {
                cars = cars.Where(c => c.Year == year.Value);
            }

            // Завантажуємо всі категорії для фільтрації
            var categories = _context.CarCategories
                .Select(c => c.Name)
                .ToList();

            ViewBag.Categories = new SelectList(categories);

            return View(cars.ToList());
        }

        [Authorize]
        public IActionResult Book(int id)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == id && c.Status == "Доступно");
            if (car == null)
            {
                return NotFound();
            }

            var model = new Reservation
            {
                CarId = id,
                StartDate = DateTime.Today, // Сьогоднішня дата
                EndDate = DateTime.Today.AddDays(1) // За замовчуванням, наступний день
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Book(Reservation reservation)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == reservation.CarId);
            if (car == null)
            {
                return NotFound();
            }

            if (reservation.StartDate >= reservation.EndDate)
            {
                ModelState.AddModelError("", "Дата початку має бути раніше дати завершення.");
                return View(reservation);
            }

            // Перевіряємо перетин дат
            var overlappingReservations = _context.Reservations
                .Where(r => r.CarId == reservation.CarId && r.Status == "Підтверджено" &&
                    (
                        (reservation.StartDate >= r.StartDate && reservation.StartDate < r.EndDate) ||
                        (reservation.EndDate > r.StartDate && reservation.EndDate <= r.EndDate) ||
                        (reservation.StartDate <= r.StartDate && reservation.EndDate >= r.EndDate)
                    ))
                .Select(r => new { r.StartDate, r.EndDate })
                .ToList();

            if (overlappingReservations.Any())
            {
                ViewBag.OverlappingDates = overlappingReservations;
                ModelState.AddModelError("", "Цей автомобіль уже заброньовано на обраний період.");
                return View(reservation);
            }

            reservation.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            reservation.TotalPrice = (decimal)((reservation.EndDate - reservation.StartDate).TotalDays) * car.PricePerDay;
            reservation.Status = "Очікує підтвердження";

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            return RedirectToAction("MyReservations");
        }


        [Authorize]
        public IActionResult MyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Car)
                .ToList();

            return View(reservations);
        }

        [HttpPost]
        [Authorize]
        public IActionResult RateCar(int carId, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                return BadRequest("Рейтинг має бути в діапазоні від 1 до 5.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Перевіряємо, чи користувач уже оцінив цей автомобіль
            var existingRating = _context.CarRatings.FirstOrDefault(r => r.CarId == carId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Rating = rating; // Оновлюємо оцінку
            }
            else
            {
                var carRating = new CarRating
                {
                    CarId = carId,
                    UserId = userId,
                    Rating = rating
                };

                _context.CarRatings.Add(carRating);
            }

            _context.SaveChanges();

            // Оновлюємо середній рейтинг автомобіля
            var car = _context.Cars.Include(c => c.CarRatings).FirstOrDefault(c => c.CarId == carId);
            if (car != null)
            {
                car.AverageRating = car.CarRatings.Any()
                    ? car.CarRatings.Average(r => r.Rating)
                    : 0;

                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = carId });
        }

    }
}
