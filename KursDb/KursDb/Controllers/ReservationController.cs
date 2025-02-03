using KursDb.Context;
using KursDb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KursDb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

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
            if (reservation == null) return NotFound();

            reservation.Status = "Підтверджено";
            _context.SaveChanges();

            return RedirectToAction(nameof(ManageReservations));
        }

        public IActionResult CancelReservation(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null) return NotFound();

            reservation.Status = "Скасовано";

            var car = _context.Cars.Find(reservation.CarId);
            if (car != null) car.Status = "Доступно";

            _context.SaveChanges();

            return RedirectToAction(nameof(ManageReservations));
        }
    }
}

