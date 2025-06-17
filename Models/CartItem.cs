namespace ElectronicsStore.Models
{
    using System.Collections.Generic; // Not directly needed for CartItem, but good practice
    // You might also need: using System.ComponentModel.DataAnnotations; if you add [Required] attribute

    public class CartItem
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; } // <<< Thêm 'required' modifier >>>
        public required string ProductImageUrl { get; set; } // <<< Thêm 'required' modifier >>>
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}