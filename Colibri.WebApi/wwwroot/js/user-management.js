// Модуль управления пользователями
const userManagement = (function() {
    let currentUserId = null;
    let currentUserName = null;
    let allUsers = [];

    // Инициализация
    function init() {
        setupFilterTags();
        allUsers = Array.from(document.querySelectorAll('#usersTableBody tr'));
        
        // Закрытие модальных окон по клику вне их
        window.addEventListener('click', function(event) {
            if (event.target.classList.contains('modal')) {
                closeModal(event.target.id);
            }
        });
    }

    // Настройка фильтров
    function setupFilterTags() {
        const tags = document.querySelectorAll('.filter-tag');
        tags.forEach(tag => {
            tag.addEventListener('click', function() {
                tags.forEach(t => t.classList.remove('active'));
                this.classList.add('active');
                filterUsers();
            });
        });
    }

    // Фильтрация пользователей
    function filterUsers() {
        const searchText = document.getElementById('searchInput').value.toLowerCase();
        const activeFilter = document.querySelector('.filter-tag.active').dataset.filter;
        
        allUsers.forEach(row => {
            const userName = row.querySelector('.user-name').textContent.toLowerCase();
            const userEmail = row.querySelector('.user-email').textContent.toLowerCase();
            const userRoles = row.dataset.roles.toLowerCase();
            const userStatus = row.dataset.status;
            
            let matchesSearch = searchText === '' || 
                userName.includes(searchText) || 
                userEmail.includes(searchText) ||
                userRoles.includes(searchText);
            
            let matchesFilter = true;
            if (activeFilter !== 'all') {
                if (activeFilter === 'active') {
                    matchesFilter = userStatus === 'active';
                } else if (activeFilter === 'locked') {
                    matchesFilter = userStatus === 'locked';
                } else {
                    matchesFilter = userRoles.includes(activeFilter);
                }
            }
            
            row.style.display = matchesSearch && matchesFilter ? '' : 'none';
        });
    }

    // Очистка поиска
    function clearSearch() {
        document.getElementById('searchInput').value = '';
        filterUsers();
    }

    // Обновление списка пользователей
    async function refreshUsers() {
        try {
            window.location.reload();
        } catch (error) {
            showNotification('Ошибка обновления списка пользователей', 'danger');
        }
    }

    // Редактирование пользователя
    async function editUser(userName) {
        try {
            const response = await fetch(`/account/user?userId=${encodeURIComponent(userName)}`);
            if (response.ok) {
                const user = await response.json();
                showNotification('Редактирование пользователя: ' + user.userName, 'info');
                // Здесь можно добавить логику редактирования
            }
        } catch (error) {
            showNotification('Ошибка загрузки данных пользователя', 'danger');
        }
    }

    // Блокировка/разблокировка пользователя
    async function toggleLock(userName) {
        if (!confirm('Вы уверены, что хотите изменить статус блокировки пользователя?')) {
            return;
        }
        
        try {
            const response = await fetch(`/account/toggle-lock/${userName}`, {
                method: 'POST'
            });
            
            if (response.ok) {
                showNotification('Статус блокировки изменен', 'success');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                showNotification('Ошибка изменения статуса блокировки', 'danger');
            }
        } catch (error) {
            showNotification('Ошибка при изменении статуса блокировки', 'danger');
        }
    }

    // Сброс пароля
    function resetPassword(userName) {
        currentUserName = userName;
        document.getElementById('resetPasswordUserName').textContent = userName;
        showModal('resetPasswordModal');
    }

    // Подтверждение сброса пароля
    async function confirmResetPassword() {
        const form = document.getElementById('resetPasswordForm');
        const newPassword = form.newPassword.value;
        const confirmPassword = form.confirmPassword.value;
        
        if (!newPassword || !confirmPassword) {
            showNotification('Заполните все поля', 'warning');
            return;
        }
        
        if (newPassword !== confirmPassword) {
            showNotification('Пароли не совпадают', 'warning');
            return;
        }
        
        try {
            const response = await fetch(`/account/reset-password/${currentUserName}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `newPassword=${encodeURIComponent(newPassword)}`
            });
            
            if (response.ok) {
                showNotification('Пароль успешно сброшен', 'success');
                closeModal('resetPasswordModal');
                form.reset();
            } else {
                showNotification('Ошибка сброса пароля', 'danger');
            }
        } catch (error) {
            showNotification('Ошибка при сбросе пароля', 'danger');
        }
    }

    // Управление ролями
    async function manageRoles(userName) {
        currentUserName = userName;
        
        try {
            const response = await fetch(`/account/user?userId=${encodeURIComponent(userName)}`);
            if (response.ok) {
                const user = await response.json();
                
                document.getElementById('userRolesInfo').innerHTML = `
                    <strong>Пользователь:</strong> ${user.userName}<br>
                    <strong>Email:</strong> ${user.email}
                `;
                
                if (user.roles && user.roles.length > 0) {
                    const rolesHtml = user.roles.map(role => `
                        <div class="d-flex justify-content-between align-items-center mb-2 p-2 border rounded">
                            <span class="role-badge role-${role.toLowerCase()}">${role}</span>
                            <button class="btn btn-sm btn-outline-danger" onclick="userManagement.removeRole('${role}')">
                                Удалить
                            </button>
                        </div>
                    `).join('');
                    document.getElementById('currentRoles').innerHTML = rolesHtml;
                } else {
                    document.getElementById('currentRoles').innerHTML = '<p class="text-muted">Роли не назначены</p>';
                }
                
                showModal('rolesModal');
            }
        } catch (error) {
            showNotification('Ошибка загрузки ролей пользователя', 'danger');
        }
    }

    // Добавление роли
    async function addRoleToUser() {
        const roleSelect = document.getElementById('roleSelect');
        const role = roleSelect.value;
        
        if (!role) {
            showNotification('Выберите роль', 'warning');
            return;
        }
        
        try {
            const response = await fetch(`/account/setroles?userId=${currentUserName}&role=${role}`, {
                method: 'POST'
            });
            
            if (response.ok) {
                showNotification(`Роль "${role}" добавлена`, 'success');
                manageRoles(currentUserName);
                roleSelect.value = '';
            } else {
                showNotification('Ошибка добавления роли', 'danger');
            }
        } catch (error) {
            showNotification('Ошибка при добавлении роли', 'danger');
        }
    }

    // Удаление роли
    async function removeRole(role) {
        if (!confirm(`Удалить роль "${role}"?`)) {
            return;
        }
        
        try {
            const response = await fetch(`/account/canselrole?userId=${currentUserName}&role=${role}`, {
                method: 'POST'
            });
            
            if (response.ok) {
                showNotification(`Роль "${role}" удалена`, 'success');
                manageRoles(currentUserName);
            } else {
                showNotification('Ошибка удаления роли', 'danger');
            }
        } catch (error) {
            showNotification('Ошибка при удалении роли', 'danger');
        }
    }

    // Добавление пользователя
    async function addUser() {
        const form = document.getElementById('addUserForm');
        const formData = new FormData(form);
        
        if (!formData.get('email') || !formData.get('password') || !formData.get('confirmPassword')) {
            showNotification('Заполните обязательные поля', 'warning');
            return;
        }
        
        if (formData.get('password') !== formData.get('confirmPassword')) {
            showNotification('Пароли не совпадают', 'warning');
            return;
        }
        
        const userData = {
            Email: formData.get('email'),
            FirstName: formData.get('firstName'),
            LastName: formData.get('lastName'),
            Password: formData.get('password'),
            Role: formData.get('role')
        };
        
        try {
            const response = await fetch('/account/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(userData)
            });
            
            if (response.ok) {
                const result = await response.text();
                showNotification('Пользователь успешно добавлен', 'success');
                closeModal('addUserModal');
                form.reset();
                setTimeout(() => window.location.reload(), 1000);
            } else {
                showNotification('Ошибка добавления пользователя', 'danger');
            }
        } catch (error) {
            showNotification('Ошибка при добавлении пользователя', 'danger');
        }
    }

    // Утилиты для работы с модальными окнами
    function showModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.add('show');
        }
    }

    function closeModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('show');
        }
    }

    function showAddUserModal() {
        showModal('addUserModal');
    }

    // Показать уведомление
    function showNotification(message, type = 'info') {
        // Используем существующий механизм уведомлений из Layout
        if (window.showNotification) {
            window.showNotification(message, type);
        } else {
            // Запасной вариант
            const notification = document.createElement('div');
            notification.className = `notification ${type} show`;
            notification.innerHTML = `
                ${message}
                <button onclick="this.parentElement.remove()" 
                        style="margin-left: 10px; background: none; border: none; cursor: pointer; font-size: 18px;">✕</button>
            `;
            
            document.body.appendChild(notification);
            
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 5000);
        }
    }

    // Публичные методы
    return {
        init,
        showAddUserModal,
        refreshUsers,
        editUser,
        toggleLock,
        resetPassword,
        confirmResetPassword,
        manageRoles,
        addRoleToUser,
        removeRole,
        addUser,
        filterUsers,
        clearSearch,
        showModal,
        closeModal
    };
})();

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    userManagement.init();
});