using System.Diagnostics;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _service;
        public HomeController(ProductService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Product()
        {
            var products = await _service.GetAllProductsAsync();
            return View(products);
        }
    }
}
