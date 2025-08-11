using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameStore.Models;
using GameStore.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace GameStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly GameStoreDBContext _context;

        public ProductsController(GameStoreDBContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------
        // ## ACTION INDEX ĐÃ ĐƯỢC CẬP NHẬT VỚI LOGIC LỌC NÂNG CAO ##
        // ------------------------------------------------------------------
        // GET: Products
        // GET: Products
        public async Task<IActionResult> Index(string searchQuery, string sortOrder)
        {
            // THAY ĐỔI DUY NHẤT Ở ĐÂY: Thay "var" bằng "IQueryable<Product>"
            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.Category)
                    .ThenInclude(c => c.Parent);

            // Xử lý lọc (phần này giữ nguyên)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchQuery) ||
                    (p.Category != null && p.Category.Name.Contains(searchQuery)) ||
                    (p.Category != null && p.Category.Parent != null && p.Category.Parent.Name.Contains(searchQuery))
                );
            }

            // Xử lý sắp xếp (phần này bây giờ sẽ không còn lỗi)
            switch (sortOrder)
            {
                case "price-asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price-desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                default:
                    productsQuery = productsQuery.OrderBy(p => p.Name); // Sắp xếp mặc định theo tên
                    break;
            }

            var products = await productsQuery.ToListAsync();

            // Xử lý yêu cầu AJAX (phần này giữ nguyên)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductGridPartial", products);
            }

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToListAsync();

            var optionCategories = new List<Category>();
            var accessoryParentCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Phụ kiện");
            if (accessoryParentCategory != null)
            {
                optionCategories = await _context.Categories
                    .Where(c => c.ParentId == accessoryParentCategory.Id)
                    .Take(4)
                    .ToListAsync();
            }

            var viewModel = new ProductDetailsViewModel
            {
                MainProduct = product,
                RelatedProducts = relatedProducts,
                OptionCategories = optionCategories,
                MainImageUrl = product.ProductImages?.FirstOrDefault(img => img.IsDefault == true)?.ImageUrl
                             ?? product.ProductImages?.FirstOrDefault()?.ImageUrl
                             ?? "/images/placeholder.png",
                AllImageUrls = product.ProductImages?.Select(img => img.ImageUrl).ToList() ?? new List<string>(),
                ReviewCount = product.Reviews?.Count ?? 0,
                AverageRating = product.Reviews?.Any() == true ? product.Reviews.Average(r => r.Rating) : 0
            };

            return View(viewModel);
        }

        #region Các action khác (Không thay đổi)
        // GET: Products/Create
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create([Bind("Name,Sku,Description,Price,StockQuantity")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Sku,Description,Price,StockQuantity,ImageUrl,Status,CategoryId,BrandId,Specifications")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
        #endregion
    }
}