using Microsoft.AspNetCore.Mvc;
using GameStore.Models;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GameStore.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace GameStore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly GameStoreDBContext _context;
        private const string CartSessionKey = "Cart";

        public CheckoutController(GameStoreDBContext context)
        {
            _context = context;
        }

        public class CartItem
        {
            public int ProductId { get; set; }
            public Product? Product { get; set; }
            public int Quantity { get; set; }
        }

        private List<CartItem> GetCart()
        {
            var cartString = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartString) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartString) ?? new List<CartItem>();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            foreach (var item in cart)
            {
                item.Product = await _context.Products.FindAsync(item.ProductId);
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = cart,
                // Lấy địa chỉ và sđt mặc định từ thông tin user nếu có
                ShippingAddress = _context.Users.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))?.Address,
                PhoneNumber = _context.Users.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))?.PhoneNumber
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CartItems = GetCart();
                foreach (var item in model.CartItems)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
                return View("Index", model);
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItemsList = GetCart();
            if (cartItemsList == null || !cartItemsList.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = int.Parse(userIdString);

            var productIds = cartItemsList.Select(item => item.ProductId).ToList();
            var productsInCart = await _context.Products
                                                .Where(p => productIds.Contains(p.Id))
                                                .ToListAsync();

            decimal totalAmount = 0;

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                ShippingAddress = model.ShippingAddress,
                PaymentMethod = model.PaymentMethod,
                Status = "Pending",
                PhoneNumber = model.PhoneNumber // Thêm số điện thoại vào order
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItemsList)
            {
                var product = productsInCart.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PriceAtPurchase = product.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                    totalAmount += product.Price * item.Quantity;
                }
            }

            order.TotalAmount = totalAmount;
            _context.Update(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction(nameof(OrderConfirmation), new { orderId = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
    }
}