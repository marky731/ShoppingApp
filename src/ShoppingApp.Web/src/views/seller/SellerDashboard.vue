<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { sellerAPI } from '../../services/api'

const router = useRouter()
const authStore = useAuthStore()

const shop = ref(null)
const stats = ref(null)
const loading = ref(true)
const error = ref(null)
const hasShop = ref(false)

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
    const shopResponse = await sellerAPI.getShop()
    if (shopResponse.data.shop) {
      shop.value = shopResponse.data.shop
      hasShop.value = true
      // Load stats if shop exists
      try {
        const statsResponse = await sellerAPI.getStats()
        stats.value = statsResponse.data
      } catch (e) {
        // Stats might fail if not approved
      }
    }
  } catch (err) {
    if (err.response?.status === 404) {
      hasShop.value = false
    } else {
      error.value = err.response?.data?.message || 'Failed to load seller data'
    }
  } finally {
    loading.value = false
  }
}

const formatPrice = (price) => `$${price?.toFixed(2) || '0.00'}`
</script>

<template>
  <div class="seller-dashboard">
    <div class="seller-sidebar">
      <div class="seller-header">
        <h2>Seller Dashboard</h2>
        <p v-if="user">{{ user.firstName }} {{ user.lastName }}</p>
      </div>
      <nav class="seller-nav">
        <router-link to="/seller" class="nav-item" exact-active-class="active">
          <i class="bi bi-house"></i>
          <span>Overview</span>
        </router-link>
        <router-link to="/seller/shop" class="nav-item" active-class="active">
          <i class="bi bi-shop"></i>
          <span>My Shop</span>
        </router-link>
        <router-link to="/seller/products" class="nav-item" active-class="active" v-if="hasShop && shop?.isApproved">
          <i class="bi bi-box-seam"></i>
          <span>Products</span>
        </router-link>
        <router-link to="/seller/orders" class="nav-item" active-class="active" v-if="hasShop && shop?.isApproved">
          <i class="bi bi-bag"></i>
          <span>Orders</span>
        </router-link>
        <router-link to="/seller/discounts" class="nav-item" active-class="active" v-if="hasShop && shop?.isApproved">
          <i class="bi bi-tag"></i>
          <span>Discounts</span>
        </router-link>
        <router-link to="/" class="nav-item">
          <i class="bi bi-arrow-left"></i>
          <span>Back to Store</span>
        </router-link>
      </nav>
    </div>

    <div class="seller-content">
      <div v-if="loading" class="loading">
        Loading...
      </div>

      <div v-else-if="error" class="error-message">
        {{ error }}
      </div>

      <div v-else>
        <!-- Overview -->
        <div v-if="$route.path === '/seller'" class="overview-section">
          <h1>Welcome to Your Seller Dashboard</h1>

          <div v-if="!hasShop" class="no-shop-message">
            <i class="bi bi-shop" style="font-size: 3rem; margin-bottom: 1rem;"></i>
            <h3>You don't have a shop yet</h3>
            <p>Create your shop to start selling products on MINIMALSHOP.</p>
            <router-link to="/seller/shop" class="create-shop-btn">
              Create Shop
            </router-link>
          </div>

          <div v-else-if="!shop.isApproved" class="pending-approval">
            <i class="bi bi-hourglass-split" style="font-size: 3rem; margin-bottom: 1rem;"></i>
            <h3>Your shop is pending approval</h3>
            <p>Your shop "{{ shop.shopName }}" is waiting for admin approval. You'll be able to add products once approved.</p>
          </div>

          <div v-else class="stats-grid">
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-currency-dollar"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ formatPrice(stats?.totalRevenue || 0) }}</div>
                <div class="stat-label">Total Revenue</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-bag-check"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats?.totalOrders || 0 }}</div>
                <div class="stat-label">Total Orders</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-clock"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats?.pendingOrders || 0 }}</div>
                <div class="stat-label">Pending Orders</div>
              </div>
            </div>
            <div class="stat-card">
              <div class="stat-icon"><i class="bi bi-box"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats?.totalProducts || 0 }}</div>
                <div class="stat-label">Products</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Router view for sub-pages -->
        <router-view v-else :shop="shop" :hasShop="hasShop" @shop-updated="loadData" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.seller-dashboard {
  display: flex;
  min-height: 100vh;
  background: var(--light-gray);
}

.seller-sidebar {
  width: 250px;
  background: var(--primary-color);
  color: #fff;
  padding: 1.5rem;
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  overflow-y: auto;
}

.seller-header {
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.seller-header h2 {
  font-size: 1.25rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.seller-header p {
  font-size: 0.9rem;
  opacity: 0.8;
}

.seller-nav {
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

.seller-content {
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

.no-shop-message, .pending-approval {
  text-align: center;
  padding: 3rem;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.no-shop-message h3, .pending-approval h3 {
  margin-bottom: 0.5rem;
}

.no-shop-message p, .pending-approval p {
  color: var(--secondary-color);
  margin-bottom: 1.5rem;
}

.create-shop-btn {
  display: inline-block;
  background: var(--primary-color);
  color: #fff;
  padding: 0.75rem 2rem;
  border-radius: 8px;
  text-decoration: none;
  transition: background 0.2s;
}

.create-shop-btn:hover {
  background: var(--secondary-color);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1.5rem;
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

.stat-icon {
  width: 60px;
  height: 60px;
  background: var(--light-gray);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  color: var(--primary-color);
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 600;
}

.stat-label {
  color: var(--secondary-color);
  font-size: 0.9rem;
}

.loading {
  text-align: center;
  padding: 3rem;
  color: var(--secondary-color);
}
</style>
