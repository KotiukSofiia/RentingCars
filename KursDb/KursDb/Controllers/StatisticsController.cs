using KursDb.Context;
using KursDb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KursDb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Statistics()
        {
            var totalReservations = _context.Reservations.Count();
            var totalIncome = _context.Reservations.Sum(r => r.TotalPrice);
            var totalCars = _context.Cars.Count();

            var model = new AdminStatisticsViewModel
            {
                TotalReservations = totalReservations,
                TotalIncome = totalIncome,
                TotalCars = totalCars
            };

            return View(model);
        }
    }
}

