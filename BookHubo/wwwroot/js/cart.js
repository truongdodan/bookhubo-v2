// Add to Cart functionality
function addToCart(bookId) {
    fetch('/api/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ bookId: bookId })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update cart count badge
            updateCartCount(data.cartCount);
            // Show success message
            showToast(data.message, 'success');
        } else {
            showToast(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
    });
}

// Update cart count badge in navbar
function updateCartCount(count) {
    const badge = document.getElementById('cartCount');
    if (badge) {
        badge.textContent = count;
        if (count > 0) {
            badge.style.display = 'inline';
        } else {
            badge.style.display = 'none';
        }
    }
}

// Show toast message
function showToast(message, type) {
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show position-fixed`;
    toast.style.cssText = 'top: 80px; right: 20px; z-index: 9999; min-width: 300px;';
    toast.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(toast);

    // Auto remove after 3 seconds
    setTimeout(() => {
        toast.remove();
    }, 3000);
}

// Load cart count on page load
document.addEventListener('DOMContentLoaded', function() {
    fetch('/api/Cart/Count')
        .then(response => response.json())
        .then(data => {
            updateCartCount(data.count);
        });

    // Handle remove from cart
    document.querySelectorAll('.btn-remove').forEach(button => {
        button.addEventListener('click', function() {
            const cartItemId = this.dataset.cartItemId;
            removeFromCart(cartItemId);
        });
    });

    // Handle increase quantity
    document.querySelectorAll('.btn-increase').forEach(button => {
        button.addEventListener('click', function() {
            const cartItemId = this.dataset.cartItemId;
            const maxStock = parseInt(this.dataset.maxStock);
            const input = document.querySelector(`.quantity-input[data-cart-item-id="${cartItemId}"]`);
            const currentQty = parseInt(input.value);

            if (currentQty < maxStock) {
                updateQuantity(cartItemId, currentQty + 1);
            } else {
                showToast('Đã đạt số lượng tối đa trong kho', 'error');
            }
        });
    });

    // Handle decrease quantity
    document.querySelectorAll('.btn-decrease').forEach(button => {
        button.addEventListener('click', function() {
            const cartItemId = this.dataset.cartItemId;
            const input = document.querySelector(`.quantity-input[data-cart-item-id="${cartItemId}"]`);
            const currentQty = parseInt(input.value);

            if (currentQty > 1) {
                updateQuantity(cartItemId, currentQty - 1);
            } else {
                showToast('Số lượng tối thiểu là 1', 'error');
            }
        });
    });
});

// Remove from cart
function removeFromCart(cartItemId) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        return;
    }

    fetch(`/api/Cart/Remove/${cartItemId}`, {
        method: 'DELETE'
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Remove row from table
            const row = document.querySelector(`tr[data-cart-item-id="${cartItemId}"]`);
            if (row) {
                row.remove();
            }

            // Update cart count
            updateCartCount(data.cartCount);

            // Show success message
            showToast(data.message, 'success');

            // Reload page if cart is empty
            if (data.cartCount === 0) {
                setTimeout(() => {
                    location.reload();
                }, 1000);
            } else {
                // Recalculate totals
                recalculateTotals();
            }
        } else {
            showToast(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
    });
}

// Update quantity
function updateQuantity(cartItemId, quantity) {
    fetch('/api/Cart/UpdateQuantity', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ cartItemId: cartItemId, quantity: quantity })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update quantity input
            const input = document.querySelector(`.quantity-input[data-cart-item-id="${cartItemId}"]`);
            input.value = quantity;

            // Update subtotal
            const row = document.querySelector(`tr[data-cart-item-id="${cartItemId}"]`);
            const subtotalCell = row.querySelector('.subtotal');
            subtotalCell.textContent = data.newSubtotal.toLocaleString('vi-VN') + ' VNĐ';

            // Recalculate totals
            recalculateTotals();

            showToast('Đã cập nhật số lượng', 'success');
        } else {
            showToast(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
    });
}

// Recalculate all totals
function recalculateTotals() {
    let grandTotal = 0;

    // For each seller group card
    document.querySelectorAll('.card').forEach(card => {
        const tbody = card.querySelector('tbody');
        if (!tbody) return;

        let groupTotal = 0;
        tbody.querySelectorAll('tr').forEach(row => {
            const subtotalText = row.querySelector('.subtotal').textContent;
            const subtotal = parseFloat(subtotalText.replace(/[^\d]/g, ''));
            groupTotal += subtotal;
        });

        // Update group subtotal
        const groupSubtotalElement = card.querySelector('h5 .text-success');
        if (groupSubtotalElement) {
            groupSubtotalElement.textContent = groupTotal.toLocaleString('vi-VN') + ' VNĐ';
        }

        grandTotal += groupTotal;
    });

    // Update grand total
    const grandTotalElement = document.getElementById('grandTotal');
    if (grandTotalElement) {
        grandTotalElement.textContent = grandTotal.toLocaleString('vi-VN') + ' VNĐ';
    }
}
