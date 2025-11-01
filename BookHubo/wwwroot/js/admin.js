// Admin ban/unban/delete functions

async function banUser(userId, userEmail) {
    if (!confirm(`Bạn có chắc chắn muốn cấm người dùng "${userEmail}"?\n\nNgười dùng sẽ không thể đăng nhập vào hệ thống.`)) {
        return;
    }

    try {
        const response = await fetch(`/api/Admin/BanUser/${userId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (data.success) {
            alert('Đã cấm người dùng thành công!');

            // Update status badge
            const statusCell = document.getElementById(`user-status-${userId}`);
            if (statusCell) {
                statusCell.innerHTML = '<span class="badge bg-dark">Bị cấm</span>';
            }

            // Update action buttons
            const actionsCell = document.getElementById(`user-actions-${userId}`);
            if (actionsCell) {
                actionsCell.innerHTML = `
                    <button class="btn btn-sm btn-success" onclick="unbanUser(${userId}, '${userEmail}')">
                        <i class="bi bi-check-circle"></i> Gỡ cấm
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteUser(${userId}, '${userEmail}')">
                        <i class="bi bi-trash"></i> Xóa
                    </button>
                `;
            }
        } else {
            alert(`Lỗi: ${data.message}`);
        }
    } catch (error) {
        console.error('Error banning user:', error);
        alert('Có lỗi xảy ra khi cấm người dùng');
    }
}

async function unbanUser(userId, userEmail) {
    if (!confirm(`Bạn có chắc chắn muốn gỡ cấm người dùng "${userEmail}"?`)) {
        return;
    }

    try {
        const response = await fetch(`/api/Admin/UnbanUser/${userId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const data = await response.json();

        if (data.success) {
            alert('Đã gỡ cấm người dùng thành công!');

            // Update status badge
            const statusCell = document.getElementById(`user-status-${userId}`);
            if (statusCell) {
                statusCell.innerHTML = '<span class="badge bg-success">Hoạt động</span>';
            }

            // Update action buttons
            const actionsCell = document.getElementById(`user-actions-${userId}`);
            if (actionsCell) {
                actionsCell.innerHTML = `
                    <button class="btn btn-sm btn-warning" onclick="banUser(${userId}, '${userEmail}')">
                        <i class="bi bi-ban"></i> Cấm
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteUser(${userId}, '${userEmail}')">
                        <i class="bi bi-trash"></i> Xóa
                    </button>
                `;
            }
        } else {
            alert(`Lỗi: ${data.message}`);
        }
    } catch (error) {
        console.error('Error unbanning user:', error);
        alert('Có lỗi xảy ra khi gỡ cấm người dùng');
    }
}

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
