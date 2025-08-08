using GameStore.Models;
using System.Collections.Generic;

namespace GameStore.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product? MainProduct { get; set; }
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
    }
}