using KursDb.Context;
using KursDb.Models;
using KursDb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KursDb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        //Для перегляду автомобілів
        public IActionResult ManageCars()
        {
            var cars = _context.Cars.ToList();
            return View(cars);
        }
        
        //Для створення автомобіля
        public IActionResult CreateCar()
        {
            var viewModel = new CarFormViewModel
            {
                Categories = _context.CarCategories
            .Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCar(CarFormViewModel viewModel)
        {
            // Видаляємо ImageUrl із ModelState для ручної обробки
            ModelState.Remove(nameof(viewModel.ImageUrl));

            // Обробляємо завантаження зображення
            string imageUrl = await HandleImageUpload(viewModel.ImageFile);
            if (imageUrl == null)
            {
                ModelState.AddModelError(nameof(viewModel.ImageFile), "Будь ласка, завантажте зображення.");
                viewModel.Categories = GetCategorySelectList();
                return View(viewModel);
            }

            // Створюємо новий автомобіль
            var car = new Car
            {
                Brand = viewModel.Brand,
                Model = viewModel.Model,
                Year = viewModel.Year,
                PricePerDay = viewModel.PricePerDay,
                Description = viewModel.Description,
                ImageUrl = imageUrl,
                CategoryId = viewModel.CategoryId
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCars));
        }        
        
        //Для редагування автомобіля
        public IActionResult EditCar(int id)
        {
            var car = _context.Cars.Find(id);
            if (car == null)
                return NotFound();

            var viewModel = new CarFormViewModel
            {
                CarId = car.CarId,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                PricePerDay = car.PricePerDay,
                Description = car.Description,
                ImageUrl = car.ImageUrl,
                CategoryId = car.CategoryId,
                Categories = _context.CarCategories
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditCar(CarFormViewModel viewModel, IFormFile ImageFile)
        {
            // Видаляємо ImageUrl із ModelState для ручної обробки
            ModelState.Remove(nameof(viewModel.ImageUrl));

            var car = _context.Cars.Find(viewModel.CarId);
            if (car == null)
                return NotFound();

            // Обробляємо завантаження нового зображення (якщо воно є)
            if (ImageFile != null)
            {
                string imageUrl = await HandleImageUpload(ImageFile, car.ImageUrl);
                car.ImageUrl = imageUrl;
            }

            // Оновлюємо інші поля автомобіля
            car.Brand = viewModel.Brand;
            car.Model = viewModel.Model;
            car.Year = viewModel.Year;
            car.PricePerDay = viewModel.PricePerDay;
            car.Description = viewModel.Description;
            car.CategoryId = viewModel.CategoryId;

            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCars));
        }        //Для видалення автомобіля
        public IActionResult DeleteCar(int id)
        {
            var car = _context.Cars.Find(id);
            if (car == null) return NotFound();

            _context.Cars.Remove(car);
            _context.SaveChanges();
            return RedirectToAction("ManageCars");
        }
        
        // методи для перегляду бронювань
        public IActionResult ManageReservations()
        {
            var reservations = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.User)
                .ToList();

            return View(reservations);
        }

        public IActionResult ConfirmReservation(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Підтверджено";
            _context.SaveChanges();

            return RedirectToAction(nameof(ManageReservations));
        }

        public IActionResult CancelReservation(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null)
                return NotFound();

            reservation.Status = "Скасовано";

            // Звільняємо автомобіль
            var car = _context.Cars.Find(reservation.CarId);
            if (car != null)
            {
                car.Status = "Доступно";
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(ManageReservations));
        }

        public IActionResult CarMaintenance(int carId)
        {
            var maintenanceRecords = _context.CarMaintenances
                .Where(m => m.CarId == carId)
                .ToList();

            ViewBag.CarId = carId;
            return View(maintenanceRecords);
        }

        [HttpPost]
        public IActionResult AddMaintenance(CarMaintenance maintenance)
        {
            _context.CarMaintenances.Add(maintenance);
            _context.SaveChanges();

            return RedirectToAction(nameof(CarMaintenance), new { carId = maintenance.CarId });
        }

        public IActionResult DeleteMaintenance(int id)
        {
            var maintenance = _context.CarMaintenances.Find(id);
            if (maintenance != null)
            {
                _context.CarMaintenances.Remove(maintenance);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(CarMaintenance), new { carId = maintenance.CarId });
        }

        public IActionResult Statistics(DateTime? startDate, DateTime? endDate)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Обчислюємо загальну кількість бронювань і дохід у одному запиті
            var reservationsData = _context.Reservations
                .Where(r => r.Status == "Підтверджено")
                .GroupBy(r => new { r.StartDate.Month, r.StartDate.Year })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalIncome = g.Sum(r => (decimal?)r.TotalPrice) ?? 0,
                    Count = g.Count()
                })
                .ToList();

            var totalReservations = reservationsData.Sum(r => r.Count);
            var monthlyReservations = reservationsData
                .FirstOrDefault(r => r.Month == currentMonth && r.Year == currentYear)?.Count ?? 0;
            var totalIncome = reservationsData.Sum(r => r.TotalIncome);
            var monthlyIncome = reservationsData
                .FirstOrDefault(r => r.Month == currentMonth && r.Year == currentYear)?.TotalIncome ?? 0;

            // Найпопулярніші автомобілі
            var mostBookedCars = _context.PopularCars
                .Select(pc => new
                {
                    Car = _context.Cars.FirstOrDefault(c => c.CarId == pc.CarId),
                    Bookings = pc.BookingCount
                })
                .ToList();

            // Загальна кількість автомобілів
            var totalCars = _context.Cars.Count();

            // Загальна вартість обслуговування
            var totalMaintenanceCost = _context.CarMaintenances
                .Sum(m => (decimal?)m.Cost) ?? 0;

            // Дохід за обраний період
            decimal? customIncome = null;
            if (startDate.HasValue && endDate.HasValue)
            {
                customIncome = _context.GetIncome(startDate.Value, endDate.Value);
            }

            // Передаємо значення в ViewBag
            ViewBag.CustomIncome = customIncome;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            // Формування моделі
            var model = new AdminStatisticsViewModel
            {
                TotalReservations = totalReservations,
                MonthlyReservations = monthlyReservations,
                TotalIncome = totalIncome,
                MonthlyIncome = monthlyIncome,
                MostBookedCars = mostBookedCars
                    .Select(c => (Car: c.Car, Bookings: c.Bookings))
                    .ToList(),
                TotalCars = totalCars,
                TotalMaintenanceCost = totalMaintenanceCost
            };

            return View(model);
        }

        private List<SelectListItem> GetCategorySelectList()
        {
            return _context.CarCategories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                }).ToList();
        }

        private async Task<string> HandleImageUpload(IFormFile imageFile, string existingImageUrl = null)
        {
            if (imageFile == null)
                return null;

            // Шлях 
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            Directory.CreateDirectory(uploadsFolder); 

            // ім'я файлу
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Завантажую файл
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // Видаляємо старе зображення, коли редагую
            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            return $"/img/{uniqueFileName}"; 
        }
    }
}
