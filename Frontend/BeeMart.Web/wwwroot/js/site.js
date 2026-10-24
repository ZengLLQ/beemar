/* ==========================================================================
   BeeMart POS - Client Logic and State Management
   ========================================================================== */

const BeeMart = {
    // API Base URL - default 5000
    apiBaseUrl: window.API_BASE_URL || 'http://localhost:5000',

    // Storage keys
    storageKeys: {
        user: 'beemart_user',
        cart: 'beemart_cart'
    },

    // Auth State Management
    getUser() {
        const raw = sessionStorage.getItem(this.storageKeys.user);
        if (!raw) return null;
        try {
            return JSON.parse(raw);
        } catch {
            return null;
        }
    },

    setUser(user) {
        if (user) {
            sessionStorage.setItem(this.storageKeys.user, JSON.stringify(user));
        } else {
            sessionStorage.removeItem(this.storageKeys.user);
        }
    },

    logout() {
        sessionStorage.removeItem(this.storageKeys.user);
        sessionStorage.removeItem(this.storageKeys.cart);
        window.location.href = '/Login';
    },

    requireAuth() {
        const user = this.getUser();
        if (!user) {
            window.location.href = '/Login';
            return null;
        }
        return user;
    },

    // Cart State Management
    getCart() {
        const raw = sessionStorage.getItem(this.storageKeys.cart);
        if (!raw) return [];
        try {
            return JSON.parse(raw);
        } catch {
            return [];
        }
    },

    setCart(cart) {
        sessionStorage.setItem(this.storageKeys.cart, JSON.stringify(cart));
    },

    clearCart() {
        sessionStorage.removeItem(this.storageKeys.cart);
    },

    // Formatters
    formatRupiah(amount) {
        const num = Number(amount) || 0;
        return 'Rp ' + Math.round(num).toLocaleString('id-ID');
    },

    formatDateTime(dateStr) {
        if (!dateStr) return '-';
        const d = new Date(dateStr);
        if (isNaN(d.getTime())) return dateStr;
        return d.toLocaleDateString('id-ID') + ' ' + d.toLocaleTimeString('id-ID');
    },

    // Toast Notification System
    showToast(message, type = 'info') {
        let container = document.getElementById('toastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toastContainer';
            container.className = 'toast-container';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        
        let icon = 'ℹ️';
        if (type === 'success') icon = '✅';
        else if (type === 'error') icon = '❌';
        else if (type === 'warning') icon = '⚠️';

        toast.innerHTML = `<span style="font-size: 1.15rem;">${icon}</span><span style="flex: 1; font-weight: 500;">${message}</span>`;
        container.appendChild(toast);

        setTimeout(() => {
            toast.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(10px)';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    },

    // Realtime Clock
    startRealtimeClock(clockElementId, userRoleNamePrefix = '') {
        const clockEl = document.getElementById(clockElementId);
        if (!clockEl) return;

        const updateClock = () => {
            const now = new Date();
            // Format: M/d/yyyy, HH:mm:ss or local format
            const dateStr = `${now.getMonth() + 1}/${now.getDate()}/${now.getFullYear()}, ${now.toTimeString().split(' ')[0]}`;
            if (userRoleNamePrefix) {
                clockEl.textContent = `${dateStr} | ${userRoleNamePrefix}`;
            } else {
                clockEl.textContent = dateStr;
            }
        };

        updateClock();
        setInterval(updateClock, 1000);
    },

    // HTTP Request helper
    async fetchApi(endpoint, options = {}) {
        const url = `${this.apiBaseUrl}${endpoint}`;
        const defaultHeaders = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };

        options.headers = { ...defaultHeaders, ...options.headers };

        try {
            const response = await fetch(url, options);
            const data = await response.json().catch(() => null);

            return {
                ok: response.ok,
                status: response.status,
                data: data
            };
        } catch (err) {
            console.error('Fetch error:', err);
            return {
                ok: false,
                status: 0,
                data: { success: false, message: 'Tidak dapat terhubung ke Backend API (' + this.apiBaseUrl + '). Pastikan API berjalan.' }
            };
        }
    }
};

window.BeeMart = BeeMart;
