using System.Diagnostics;
using GameStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameStore.ViewModels; // Thêm dòng này

namespace GameStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly GameStoreDBContext _context;

        public HomeController(GameStoreDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchQuery)
        {
            var products = await _context.Products.ToListAsync();
            // Sửa lỗi: So sánh với giá trị true một cách rõ ràng
            var discounts = await _context.Discounts.Where(d => d.IsActive == true).ToListAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchQuery.ToLower())).ToList();
            }

            var viewModel = new HomeViewModel
            {
                Products = products,
                Discounts = discounts,
                SearchQuery = searchQuery
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}