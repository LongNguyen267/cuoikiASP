using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameStore.Models;
using GameStore.ViewModels;

namespace GameStore.Controllers
{
    [Authorize(Roles = "Employee,Admin")]
    public class EmployeeController : Controller
    {
        private readonly GameStoreDBContext _context;

        public EmployeeController(GameStoreDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalProductsInStock = await _context.Products.SumAsync(p => p.StockQuantity);

            var viewModel = new EmployeeDashboardViewModel
            {
                TotalProductsInStock = totalProductsInStock
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ViewOrders(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                                        .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                                        .OrderByDescending(o => o.OrderDate)
                                        .ToListAsync();

            var viewModel = new EmployeeDashboardViewModel
            {
                Orders = orders
            };

            return View("Index", viewModel);
        }
    }
}