<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { adminAPI } from '../../services/api'

const router = useRouter()
const authStore = useAuthStore()

const stats = ref(null)
const loading = ref(true)
const error = ref(null)

const isAuthenticated = computed(() => authStore.isAuthenticated)
const user = computed(() => authStore.currentUser)

onMounted(async () => {
  if (!isAuthenticated.value) {
    router.push('/login')
    return
  }
  await loadData()
})

const loadData = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await adminAPI.getStats()
    stats.value = response.data
  } catch (err) {
    if (err.response?.status === 403) {
      error.value = 'Access denied. Admin privileges required.'
    } else {
      error.value = err.response?.data?.message || 'Failed to load admin data'
    }
  } finally {
    loading.value = false
  }
}

const formatPrice = (price) => `$${price?.toFixed(2) || '0.00'}`
</script>

<template>
  <div class="admin-dashboard">
    <div class="admin-sidebar">
      <div class="admin-header">
        <h2>Admin Dashboard</h2>
        <p v-if="user">{{ user.firstName }} {{ user.lastName }}</p>
      </div>
      <nav class="admin-nav">
        <router-link to="/admin" class="nav-item" exact-active-class="active">
          <i class="bi bi-speedometer2"></i>
          <span>Overview</span>
        </router-link>
        <router-link to="/admin/users" class="nav-item" active-class="active">
          <i class="bi bi-people"></i>
          <span>Users</span>
        </router-link>
        <router-link to="/admin/sellers" class="nav-item" active-class="active">
          <i class="bi bi-shop-window"></i>
          <span>Pending Sellers</span>
        </router-link>
        <router-link to="/admin/categories" class="nav-item" active-class="active">
          <i class="bi bi-grid"></i>
          <span>Categories</span>
        </router-link>
        <router-link to="/admin/reviews" class="nav-item" active-class="active">
          <i class="bi bi-chat-square-text"></i>
          <span>Review Moderation</span>
        </router-link>
        <router-link to="/" class="nav-item">
          <i class="bi bi-arrow-left"></i>
          <span>Back to Store</span>
        </router-link>
      </nav>
    </div>

    <div class="admin-content">
      <div v-if="loading" class="loading">
        Loading...
      </div>

      <div v-else-if="error" class="error-message">
        {{ error }}
      </div>

      <div v-else>
        <!-- Overview -->
        <div v-if="$route.path === '/admin'" class="overview-section">
          <h1>Admin Overview</h1>

          <div class="stats-grid" v-if="stats">
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-people"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats.totalUsers }}</div>
                <div class="stat-label">Total Users</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-shop"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats.totalSellers }}</div>
                <div class="stat-label">Active Sellers</div>
              </div>
            </div>
            <div class="stat-card alert" v-if="stats.pendingSellers > 0">
              <div class="stat-icon"><i class="bi bi-hourglass-split"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats.pendingSellers }}</div>
                <div class="stat-label">Pending Sellers</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-box-seam"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats.totalProducts }}</div>
                <div class="stat-label">Products</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-bag-check"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats.totalOrders }}</div>
                <div class="stat-label">Total Orders</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-currency-dollar"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ formatPrice(stats.totalRevenue) }}</div>
                <div class="stat-label">Total Revenue</div>
              </div>
            </div>
            <div class="stat-card alert" v-if="stats.pendingReviews > 0">
              <div class="stat-icon"><i class="bi bi-chat-square-text"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats.pendingReviews }}</div>
                <div class="stat-label">Pending Reviews</div>
              </div>
            </div>
          </div>

          <div class="quick-actions" v-if="stats && (stats.pendingSellers > 0 || stats.pendingReviews > 0)">
            <h3>Quick Actions</h3>
            <div class="action-cards">
              <router-link v-if="stats.pendingSellers > 0" to="/admin/sellers" class="action-card">
                <i class="bi bi-shop-window"></i>
                <span>Review {{ stats.pendingSellers }} pending seller{{ stats.pendingSellers > 1 ? 's' : '' }}</span>
              </router-link>
              <router-link v-if="stats.pendingReviews > 0" to="/admin/reviews" class="action-card">
                <i class="bi bi-chat-square-text"></i>
                <span>Moderate {{ stats.pendingReviews }} pending review{{ stats.pendingReviews > 1 ? 's' : '' }}</span>
              </router-link>
            </div>
          </div>
        </div>

        <!-- Router view for sub-pages -->
        <router-view v-else @refresh="loadData" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-dashboard {
  display: flex;
  min-height: 100vh;
  background: var(--light-gray);
}

.admin-sidebar {
  width: 250px;
  background: #1a1a2e;
  color: #fff;
  padding: 1.5rem;
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  overflow-y: auto;
}

.admin-header {
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.admin-header h2 {
  font-size: 1.25rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.admin-header p {
  font-size: 0.9rem;
  opacity: 0.8;
}

.admin-nav {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  color: rgba(255, 255, 255, 0.8);
  text-decoration: none;
  border-radius: 8px;
  transition: all 0.2s;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #fff;
}

.nav-item.active {
  background: rgba(255, 255, 255, 0.2);
  color: #fff;
}

.nav-item i {
  font-size: 1.1rem;
}

.admin-content {
  flex: 1;
  margin-left: 250px;
  padding: 2rem;
}

.overview-section {
  max-width: 1200px;
}

.overview-section h1 {
  font-weight: 400;
  margin-bottom: 2rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: #fff;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-card.alert {
  border-left: 4px solid #ffc107;
}

.stat-icon {
  width: 60px;
  height: 60px;
  background: var(--light-gray);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  color: #1a1a2e;
}

.stat-card.alert .stat-icon {
  background: #fff3cd;
  color: #856404;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 600;
}

.stat-label {
  color: var(--secondary-color);
  font-size: 0.9rem;
}

.quick-actions {
  margin-top: 2rem;
}

.quick-actions h3 {
  font-weight: 500;
  margin-bottom: 1rem;
}

.action-cards {
  display: flex;
  gap: 1rem;
}

.action-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  background: #fff;
  border: 1px solid #ffc107;
  border-radius: 8px;
  text-decoration: none;
  color: #856404;
  transition: all 0.2s;
}

.action-card:hover {
  background: #fff3cd;
}

.loading {
  text-align: center;
  padding: 3rem;
  color: var(--secondary-color);
}
</style>
