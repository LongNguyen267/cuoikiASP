// Thư mục: ViewModels/ProductDetailsViewModel.cs

using GameStore.Models;
using System.Collections.Generic;

namespace GameStore.ViewModels
{
    public class ProductDetailsViewModel
    {
        // Dữ liệu gốc
        public Product MainProduct { get; set; } = new Product();
        public List<Product> RelatedProducts { get; set; } = new List<Product>();

        // <<-- THÊM DÒNG NÀY -->>
        // Thuộc tính để chứa các danh mục tùy chọn/phụ kiện
        public List<Category> OptionCategories { get; set; } = new List<Category>();

        // Dữ liệu đã được xử lý sẵn để View hiển thị
        public string MainImageUrl { get; set; } = "";
        public List<string> AllImageUrls { get; set; } = new List<string>();
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }
}