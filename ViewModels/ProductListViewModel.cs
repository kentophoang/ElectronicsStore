// ViewModels/ProductListViewModel.cs
using ElectronicsStore.Models;
using System.Collections.Generic;

namespace ElectronicsStore.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public int? CurrentCategoryId { get; set; }
        public string? CurrentCategoryName { get; set; }
        public string? SearchString { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}