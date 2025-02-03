using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly ICarService _carService;
    private readonly IReservationService _reservationService;
    private readonly IRatingService _ratingService;

    public HomeController(ICarService carService, IReservationService reservationService, IRatingService ratingService)
    {
        _carService = carService;
        _reservationService = reservationService;
        _ratingService = ratingService;
    }

    public IActionResult Index()
    {
        var cars = _carService.GetAvailableCars(null, null, null, null);
        return View(cars);
    }

    public IActionResult Details(int id)
    {
        var car = _carService.GetCarById(id);
        if (car == null) return NotFound();

        return View(car);
    }

    [Authorize]
    public IActionResult MyReservations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reservations = _reservationService.GetUserReservations(userId);
        return View(reservations);
    }

    [Authorize]
    [HttpPost]
    public IActionResult Book(int carId, DateTime startDate, DateTime endDate)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            var reservation = _reservationService.BookCar(carId, userId, startDate, endDate);
            return RedirectToAction("MyReservations");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }

    [HttpPost]
[Authorize]
public IActionResult RateCar(int carId, int rating)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    try
    {
        _ratingService.RateCar(carId, userId, rating);
        return RedirectToAction("Details", new { id = carId });
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
}
