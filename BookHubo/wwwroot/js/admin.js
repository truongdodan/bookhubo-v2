// Admin delete functions

async function deleteUser(userId, userEmail) {
    if (!confirm(`Bạn có chắc chắn muốn xóa người dùng "${userEmail}"?\n\nHành động này sẽ xóa vĩnh viễn người dùng và tất cả dữ liệu liên quan (sách, đơn hàng, v.v.)`)) {
        return;
    }

    try {
        const response = await fetch(`/api/Admin/DeleteUser/${userId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (data.success) {
            alert('Đã xóa người dùng thành công!');
            // Remove row from table
            const row = document.getElementById(`user-row-${userId}`);
            if (row) {
                row.remove();
            }
        } else {
            alert(`Lỗi: ${data.message}`);
        }
    } catch (error) {
        console.error('Error deleting user:', error);
        alert('Có lỗi xảy ra khi xóa người dùng');
    }
}

async function deleteListing(bookId, bookTitle) {
    if (!confirm(`Bạn có chắc chắn muốn xóa sách "${bookTitle}"?\n\nSách sẽ bị ẩn khỏi danh sách (soft delete).`)) {
        return;
    }

    try {
        const response = await fetch(`/api/Admin/DeleteListing/${bookId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (data.success) {
            alert('Đã xóa sách thành công!');
            // Update row status
            const row = document.getElementById(`listing-row-${bookId}`);
            if (row) {
                // Update status badge
                const statusCell = row.querySelector('td:nth-child(5)');
                statusCell.innerHTML = '<span class="badge bg-secondary">Đã xóa</span>';

                // Update action button
                const actionCell = row.querySelector('td:nth-child(7)');
                actionCell.innerHTML = '<span class="text-muted">Đã xóa</span>';
            }
        } else {
            alert(`Lỗi: ${data.message}`);
        }
    } catch (error) {
        console.error('Error deleting listing:', error);
        alert('Có lỗi xảy ra khi xóa sách');
    }
}
