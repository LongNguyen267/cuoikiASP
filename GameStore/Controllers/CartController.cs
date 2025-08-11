// Nội dung mới cho file: Controllers/CartController.cs

using Microsoft.AspNetCore.Mvc;
using GameStore.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameStore.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";
        private readonly GameStoreDBContext _context;

        public CartController(GameStoreDBContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ session
        private List<CartItem> GetCart()
        {
            var cartString = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartString) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartString) ?? new List<CartItem>();
        }

        // Lưu giỏ hàng vào session
        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        // Action hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCart();

            // Lấy thông tin sản phẩm đầy đủ từ database để hiển thị
            // Đây là lúc chúng ta gán đối tượng Product vào
            foreach (var item in cart)
            {
                item.Product = _context.Products.Find(item.ProductId);
            }
            return View(cart);
        }

        // Action thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                // Nếu sản phẩm đã có, chỉ tăng số lượng
                cartItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có, thêm mới item vào giỏ
                // Không cần gán Product ở đây
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
            // Chuyển hướng về trang sản phẩm hoặc trang giỏ hàng
            return RedirectToAction("Index", "Products");
        }

        // Action xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}