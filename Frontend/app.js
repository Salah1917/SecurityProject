const API_URL = '/api';

// State Management
let currentUser = null;
let accessToken = localStorage.getItem('accessToken');

// Initialize App
document.addEventListener('DOMContentLoaded', () => {
    if (accessToken) {
        checkAuth();
    } else {
        showAuth();
    }
});

// Navigation Logic
function showTab(tab) {
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.add('hidden'));
    
    const activeBtn = document.querySelector(`.tab-btn[onclick="showTab('${tab}')"]`);
    const activeContent = document.getElementById(`${tab}-form`);
    
    activeBtn.classList.add('active');
    activeContent.classList.remove('hidden');
}

function showAuth() {
    document.getElementById('auth-section').classList.remove('hidden');
    document.getElementById('dashboard-section').classList.add('hidden');
}

function showDashboard() {
    document.getElementById('auth-section').classList.add('hidden');
    document.getElementById('dashboard-section').classList.remove('hidden');
    updateDashboardUI();
}

// Auth Actions
async function handleLogin(e) {
    e.preventDefault();
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;

    try {
        const response = await fetch(`${API_URL}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();

        if (response.ok) {
            accessToken = data.accessToken;
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', data.refreshToken);
            showToast('Login successful!', 'success');
            checkAuth();
        } else {
            showToast(data.message || 'Login failed', 'error');
        }
    } catch (err) {
        showToast('Network error', 'error');
    }
}

async function handleRegister(e) {
    e.preventDefault();
    const username = document.getElementById('reg-username').value;
    const email = document.getElementById('reg-email').value;
    const password = document.getElementById('reg-password').value;
    const role = document.getElementById('reg-role').value;

    try {
        const response = await fetch(`${API_URL}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password, role })
        });

        if (response.ok) {
            showToast('Account created! Please login.', 'success');
            showTab('login');
        } else {
            const data = await response.json();
            showToast(data.message || 'Registration failed', 'error');
        }
    } catch (err) {
        showToast('Network error', 'error');
    }
}

async function checkAuth() {
    try {
        const response = await fetch(`${API_URL}/test/me`, {
            headers: { 'Authorization': `Bearer ${accessToken}` }
        });

        if (response.ok) {
            currentUser = await response.json();
            showDashboard();
        } else {
            handleLogout();
        }
    } catch (err) {
        handleLogout();
    }
}

function handleLogout() {
    currentUser = null;
    accessToken = null;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    showAuth();
}

// UI Updates
function updateDashboardUI() {
    if (!currentUser) return;

    document.getElementById('user-display-name').textContent = currentUser.username;
    document.getElementById('user-display-email').textContent = currentUser.email;

    // Update Roles
    const rolesContainer = document.getElementById('roles-list');
    rolesContainer.innerHTML = '';
    currentUser.roles.forEach(role => {
        const badge = document.createElement('span');
        badge.className = 'role-badge';
        badge.textContent = role;
        rolesContainer.appendChild(badge);
    });

    // Update Permissions
    const permsContainer = document.getElementById('permissions-list');
    permsContainer.innerHTML = '';
    
    // We define what we want to check
    const allPossiblePerms = ['read', 'write', 'delete', 'manage_users'];
    
    allPossiblePerms.forEach(perm => {
        const hasIt = currentUser.permissions.includes(perm);
        const item = document.createElement('div');
        item.className = `perm-item ${hasIt ? '' : 'denied'}`;
        item.innerHTML = `
            <span>${hasIt ? '✅' : '❌'}</span>
            <span>${perm.replace('_', ' ')}</span>
        `;
        permsContainer.appendChild(item);
    });
}

// API Testing
async function testEndpoint(endpoint) {
    const responseBox = document.getElementById('api-response');
    responseBox.innerHTML = '<p class="placeholder">Calling API...</p>';

    const urls = {
        'public': '/test/public',
        'protected': '/test/protected',
        'admin': '/test/admin',
        'management': '/test/management'
    };

    try {
        const response = await fetch(`${API_URL}${urls[endpoint]}`, {
            headers: { 'Authorization': `Bearer ${accessToken}` }
        });

        const contentType = response.headers.get("content-type");
        let data;
        
        if (contentType && contentType.includes("application/json")) {
            data = await response.json();
        } else {
            data = { message: await response.text() };
        }
        
        responseBox.innerHTML = `
            <p class="${response.ok ? 'success-text' : 'error-text'}">
                Status: ${response.status} ${response.statusText}
            </p>
            <pre>${JSON.stringify(data, null, 2)}</pre>
        `;
    } catch (err) {
        responseBox.innerHTML = `<p class="error-text">Network error: ${err.message}</p>`;
    }
}

// Utility
function showToast(message, type = 'info') {
    const container = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    
    if (type === 'success') toast.style.borderLeftColor = 'var(--success)';
    if (type === 'error') toast.style.borderLeftColor = 'var(--error)';
    
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}
