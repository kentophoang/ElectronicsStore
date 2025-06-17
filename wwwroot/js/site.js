document.addEventListener('DOMContentLoaded', function () {
    // Example for product details image gallery (already in Details.cshtml section)
    // $('.product-detail-images img').on('click', function() {
    //     $('.product-detail-images img').removeClass('active');
    //     $(this).addClass('active');
    //     $('#mainProductImage').attr('src', $(this).attr('src'));
    // });

    // Handle Add to Cart button (example - will need AJAX for real implementation)
    // This is moved to the Details.cshtml section script for specific product ID.
    // However, for product cards on Index.cshtml, you can still use this generic one.
    $('button.btn-add-to-cart').on('click', function (e) {
        e.preventDefault();
        var productId = $(this).data('product-id'); // Get product ID from data attribute

        if (productId) {
            // Make AJAX call to add to cart for products listed on Index page
            $.ajax({
                url: '/Cart/AddToCart',
                type: 'POST',
                data: { productId: productId, quantity: 1 }, // Default to 1
                success: function (response) {
                    if (response.success) {
                        alert('Đã thêm sản phẩm vào giỏ hàng!');
                        // Update cart count in header
                        $('.fa-shopping-cart').next('span').text(response.cartItemCount);
                    } else {
                        alert('Có lỗi xảy ra: ' + response.message);
                    }
                },
                error: function (xhr, status, error) {
                    alert('Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng.');
                    console.error(xhr.responseText);
                }
            });
        }
    });

    // Handle Quantity +/- on cart page (already in Cart/Index.cshtml section script)
    // $('.quantity-btn').on('click', function() {
    //     var input = $(this).siblings('.quantity-input');
    //     var currentVal = parseInt(input.val());
    //     var change = $(this).data('change'); // +1 or -1

    //     var newVal = currentVal + change;
    //     if (newVal >= 1) { // Ensure quantity is at least 1
    //         input.val(newVal);
    //         // You'll need to trigger an update to the server here
    //         // to re-calculate cart totals. This would likely be an AJAX call.
    //     }
    // });

    // Live search functionality (example)
    // You'd attach this to your search input and make AJAX calls
    // to a search API endpoint on the server.
    // $('#searchInput').on('keyup', function() {
    //    var query = $(this).val();
    //    if (query.length > 2) {
    //        $.get('/Product/LiveSearch?query=' + query, function(data) {
    //            // Display results in a dropdown or overlay
    //        });
    //    }
    // });
});
