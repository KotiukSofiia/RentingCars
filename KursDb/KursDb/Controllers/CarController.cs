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
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ManageCars()
        {
            var cars = _context.Cars.ToList();
            return View(cars);
        }

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
            ModelState.Remove(nameof(viewModel.ImageUrl));

            string imageUrl = await HandleImageUpload(viewModel.ImageFile);
            if (imageUrl == null)
            {
                ModelState.AddModelError(nameof(viewModel.ImageFile), "Будь ласка, завантажте зображення.");
                viewModel.Categories = GetCategorySelectList();
                return View(viewModel);
            }

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

        public IActionResult EditCar(int id)
        {
            var car = _context.Cars.Find(id);
            if (car == null) return NotFound();

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
            ModelState.Remove(nameof(viewModel.ImageUrl));

            var car = _context.Cars.Find(viewModel.CarId);
            if (car == null) return NotFound();

            if (ImageFile != null)
            {
                string imageUrl = await HandleImageUpload(ImageFile, car.ImageUrl);
                car.ImageUrl = imageUrl;
            }

            car.Brand = viewModel.Brand;
            car.Model = viewModel.Model;
            car.Year = viewModel.Year;
            car.PricePerDay = viewModel.PricePerDay;
            car.Description = viewModel.Description;
            car.CategoryId = viewModel.CategoryId;

            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCars));
        }

        public IActionResult DeleteCar(int id)
        {
            var car = _context.Cars.Find(id);
            if (car == null) return NotFound();

            _context.Cars.Remove(car);
            _context.SaveChanges();
            return RedirectToAction(nameof(ManageCars));
        }

        private async Task<string> HandleImageUpload(IFormFile imageFile, string existingImageUrl = null)
        {
            if (imageFile == null) return null;

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            if (!string.IsNullOrEmpty(existingImageUrl))
            {
                string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            return $"/img/{uniqueFileName}";
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
    }
}

