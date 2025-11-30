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
const success = ref(null)
const hasShop = ref(false)
const isEditingShop = ref(false)
const shopFormLoading = ref(false)

const shopForm = ref({
  shopName: '',
  description: '',
  logoImageUrl: '',
  bannerImageUrl: ''
})

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
    if (shopResponse.data) {
      shop.value = shopResponse.data
      hasShop.value = true
      // Populate form with current shop data
      shopForm.value = {
        shopName: shop.value.shopName || '',
        description: shop.value.description || '',
        logoImageUrl: shop.value.logoImageUrl || '',
        bannerImageUrl: shop.value.bannerImageUrl || ''
      }
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

const openEditShop = () => {
  if (hasShop.value) {
    shopForm.value = {
      shopName: shop.value.shopName || '',
      description: shop.value.description || '',
      logoImageUrl: shop.value.logoImageUrl || '',
      bannerImageUrl: shop.value.bannerImageUrl || ''
    }
  }
  isEditingShop.value = true
}

const cancelEdit = () => {
  isEditingShop.value = false
  success.value = null
  error.value = null
}

const saveShop = async () => {
  if (!shopForm.value.shopName.trim()) {
    error.value = 'Shop name is required'
    return
  }

  shopFormLoading.value = true
  error.value = null
  success.value = null

  try {
    if (hasShop.value) {
      await sellerAPI.updateShop(shopForm.value)
      success.value = 'Shop updated successfully'
    } else {
      await sellerAPI.createShop(shopForm.value)
      success.value = 'Shop created successfully! Waiting for admin approval.'
    }
    await loadData()
    setTimeout(() => {
      isEditingShop.value = false
      success.value = null
    }, 1500)
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save shop'
  } finally {
    shopFormLoading.value = false
  }
}

const navigateTo = (path) => {
  router.push(path)
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

      <div v-else>
        <!-- Overview -->
        <div v-if="$route.path === '/seller'" class="overview-section">
          <h1>Welcome back, {{ user?.firstName }}!</h1>

          <!-- No Shop Yet -->
          <div v-if="!hasShop" class="no-shop-message">
            <i class="bi bi-shop"></i>
            <h3>You don't have a shop yet</h3>
            <p>Create your shop to start selling products on MINIMALSHOP.</p>
            <button class="create-shop-btn" @click="isEditingShop = true">
              <i class="bi bi-plus-lg"></i> Create Shop
            </button>
          </div>

          <!-- Shop Profile Card (Read-only) -->
          <div v-else class="shop-profile-card clickable" @click="openEditShop">
            <div class="shop-profile-header">
              <div class="shop-logo">
                <img v-if="shop.logoImageUrl" :src="shop.logoImageUrl" :alt="shop.shopName" />
                <i v-else class="bi bi-shop"></i>
              </div>
              <div class="shop-details">
                <h2>{{ shop.shopName }}</h2>
                <p class="shop-description">{{ shop.description || 'No description' }}</p>
                <div class="shop-badges">
                  <span v-if="shop.isApproved" class="badge approved">
                    <i class="bi bi-check-circle"></i> Approved
                  </span>
                  <span v-else class="badge pending">
                    <i class="bi bi-hourglass-split"></i> Pending Approval
                  </span>
                  <span class="badge rating" v-if="shop.averageRating">
                    <i class="bi bi-star-fill"></i> {{ shop.averageRating.toFixed(1) }}
                  </span>
                </div>
              </div>
              <div class="edit-hint">
                <i class="bi bi-pencil"></i>
                <span>Click to edit</span>
              </div>
            </div>
          </div>

          <!-- Pending Approval Message -->
          <div v-if="hasShop && !shop.isApproved" class="pending-notice">
            <i class="bi bi-info-circle"></i>
            <span>Your shop is pending admin approval. You'll be able to add products once approved.</span>
          </div>

          <!-- Stats Grid (Only if approved) -->
          <div v-if="hasShop && shop.isApproved" class="stats-grid">
            <div class="stat-card clickable" @click="navigateTo('/seller/orders')">
              <div class="stat-icon revenue"><i class="bi bi-currency-dollar"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ formatPrice(stats?.totalRevenue || 0) }}</div>
                <div class="stat-label">Total Revenue</div>
              </div>
              <i class="bi bi-chevron-right stat-arrow"></i>
            </div>
            <div class="stat-card clickable" @click="navigateTo('/seller/orders')">
              <div class="stat-icon orders"><i class="bi bi-bag-check"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats?.totalOrders || 0 }}</div>
                <div class="stat-label">Total Orders</div>
              </div>
              <i class="bi bi-chevron-right stat-arrow"></i>
            </div>
            <div class="stat-card clickable" @click="navigateTo('/seller/orders?status=pending')">
              <div class="stat-icon pending"><i class="bi bi-clock"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats?.pendingOrders || 0 }}</div>
                <div class="stat-label">Pending Orders</div>
              </div>
              <i class="bi bi-chevron-right stat-arrow"></i>
            </div>
            <div class="stat-card clickable" @click="navigateTo('/seller/products')">
              <div class="stat-icon products"><i class="bi bi-box"></i></div>
              <div class="stat-info">
                <div class="stat-value">{{ stats?.totalProducts || 0 }}</div>
                <div class="stat-label">Products</div>
              </div>
              <i class="bi bi-chevron-right stat-arrow"></i>
            </div>
          </div>
        </div>

        <!-- Router view for sub-pages -->
        <router-view v-else :shop="shop" :hasShop="hasShop" @shop-updated="loadData" />
      </div>
    </div>

    <!-- Shop Edit Modal -->
    <div v-if="isEditingShop" class="modal-overlay" @click.self="cancelEdit">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ hasShop ? 'Edit Shop Profile' : 'Create Your Shop' }}</h2>
          <button class="close-btn" @click="cancelEdit">&times;</button>
        </div>

        <div v-if="error" class="error-message">{{ error }}</div>
        <div v-if="success" class="success-message">{{ success }}</div>

        <div class="form-group">
          <label>Shop Name *</label>
          <input v-model="shopForm.shopName" type="text" placeholder="Enter your shop name" />
        </div>

        <div class="form-group">
          <label>Description</label>
          <textarea v-model="shopForm.description" rows="3" placeholder="Tell customers about your shop"></textarea>
        </div>

        <div class="form-group">
          <label>Logo Image URL</label>
          <input v-model="shopForm.logoImageUrl" type="url" placeholder="https://..." />
          <div v-if="shopForm.logoImageUrl" class="image-preview">
            <img :src="shopForm.logoImageUrl" alt="Logo preview" />
          </div>
        </div>

        <div class="form-group">
          <label>Banner Image URL</label>
          <input v-model="shopForm.bannerImageUrl" type="url" placeholder="https://..." />
          <div v-if="shopForm.bannerImageUrl" class="image-preview banner">
            <img :src="shopForm.bannerImageUrl" alt="Banner preview" />
          </div>
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="cancelEdit">Cancel</button>
          <button class="save-btn" @click="saveShop" :disabled="shopFormLoading">
            {{ shopFormLoading ? 'Saving...' : (hasShop ? 'Update Shop' : 'Create Shop') }}
          </button>
        </div>
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
  margin-bottom: 1.5rem;
}

/* No Shop Message */
.no-shop-message {
  text-align: center;
  padding: 3rem;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.no-shop-message i {
  font-size: 3rem;
  color: var(--secondary-color);
  margin-bottom: 1rem;
}

.no-shop-message h3 {
  margin-bottom: 0.5rem;
}

.no-shop-message p {
  color: var(--secondary-color);
  margin-bottom: 1.5rem;
}

.create-shop-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--primary-color);
  color: #fff;
  padding: 0.75rem 2rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.2s;
}

.create-shop-btn:hover {
  background: var(--secondary-color);
}

/* Shop Profile Card */
.shop-profile-card {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.shop-profile-card.clickable {
  cursor: pointer;
  transition: box-shadow 0.2s, transform 0.2s;
}

.shop-profile-card.clickable:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.shop-profile-header {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.shop-logo {
  width: 80px;
  height: 80px;
  border-radius: 12px;
  overflow: hidden;
  background: var(--light-gray);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.shop-logo img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.shop-logo i {
  font-size: 2rem;
  color: var(--secondary-color);
}

.shop-details {
  flex: 1;
}

.shop-details h2 {
  font-weight: 500;
  margin-bottom: 0.25rem;
}

.shop-description {
  color: var(--secondary-color);
  font-size: 0.9rem;
  margin-bottom: 0.75rem;
}

.shop-badges {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.badge {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  font-size: 0.8rem;
}

.badge.approved {
  background: #d4edda;
  color: #155724;
}

.badge.pending {
  background: #fff3cd;
  color: #856404;
}

.badge.rating {
  background: #fff3cd;
  color: #856404;
}

.badge.rating i {
  color: #ffc107;
}

.edit-hint {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
  color: var(--secondary-color);
  font-size: 0.85rem;
  opacity: 0;
  transition: opacity 0.2s;
}

.shop-profile-card:hover .edit-hint {
  opacity: 1;
}

/* Pending Notice */
.pending-notice {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: #fff3cd;
  color: #856404;
  padding: 1rem 1.5rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
}

/* Stats Grid */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
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

.stat-card.clickable {
  cursor: pointer;
  transition: box-shadow 0.2s, transform 0.2s;
}

.stat-card.clickable:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}

.stat-icon.revenue {
  background: #d4edda;
  color: #155724;
}

.stat-icon.orders {
  background: #cce5ff;
  color: #004085;
}

.stat-icon.pending {
  background: #fff3cd;
  color: #856404;
}

.stat-icon.products {
  background: #e2e3e5;
  color: #383d41;
}

.stat-info {
  flex: 1;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 600;
}

.stat-label {
  color: var(--secondary-color);
  font-size: 0.9rem;
}

.stat-arrow {
  color: var(--secondary-color);
  font-size: 1rem;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: #fff;
  padding: 2rem;
  border-radius: 12px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.modal-header h2 {
  font-weight: 500;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: var(--secondary-color);
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group input, .form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  font-size: 1rem;
}

.form-group input:focus, .form-group textarea:focus {
  outline: none;
  border-color: var(--primary-color);
}

.image-preview {
  margin-top: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  overflow: hidden;
  display: inline-block;
}

.image-preview img {
  width: 80px;
  height: 80px;
  object-fit: cover;
}

.image-preview.banner img {
  width: 100%;
  height: 120px;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.cancel-btn, .save-btn {
  flex: 1;
  padding: 0.75rem;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
}

.cancel-btn {
  background: #fff;
  border: 1px solid var(--border-color);
}

.save-btn {
  background: var(--primary-color);
  color: #fff;
  border: none;
}

.save-btn:hover:not(:disabled) {
  background: var(--secondary-color);
}

.save-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.success-message {
  background: #d4edda;
  color: #155724;
  padding: 0.75rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.error-message {
  background: #f8d7da;
  color: #721c24;
  padding: 0.75rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.loading {
  text-align: center;
  padding: 3rem;
  color: var(--secondary-color);
}
</style>
