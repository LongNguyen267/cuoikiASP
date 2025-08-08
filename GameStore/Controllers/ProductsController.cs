using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameStore.Models;
using GameStore.ViewModels; // Thêm dòng này

namespace GameStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly GameStoreDBContext _context;

        public ProductsController(GameStoreDBContext context)
        {
            _context = context;
        }

        // GET: Products
        // Mọi người đều có thể xem danh sách sản phẩm, không cần phân quyền
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        // Mọi người đều có thể xem chi tiết sản phẩm, không cần phân quyền
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mainProduct = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);

            if (mainProduct == null)
            {
                return NotFound();
            }

            // Lấy danh sách sản phẩm liên quan (cùng danh mục)
            var relatedProducts = await _context.Products
                                                .Where(p => p.CategoryId == mainProduct.CategoryId && p.Id != mainProduct.Id)
                                                .Take(4) // Chỉ lấy 4 sản phẩm
                                                .ToListAsync();

            var viewModel = new ProductDetailsViewModel
            {
                MainProduct = mainProduct,
                RelatedProducts = relatedProducts
            };

            return View(viewModel);
        }

        // GET: Products/Create
        // Chỉ Admin và Employee mới có quyền tạo sản phẩm
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // Chỉ Admin và Employee mới có quyền tạo sản phẩm
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
        // Chỉ Admin và Employee mới có quyền chỉnh sửa sản phẩm
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
        // Chỉ Admin và Employee mới có quyền chỉnh sửa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Sku,Description,Price,StockQuantity,ImageUrl,Status,CategoryID,BrandID")] Product product)
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

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // GET: Products/Delete/5
        // Chỉ Admin và Employee mới có quyền xóa sản phẩm
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
        // Chỉ Admin và Employee mới có quyền xóa sản phẩm
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
    }
}