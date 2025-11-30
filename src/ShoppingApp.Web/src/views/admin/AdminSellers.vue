<script setup>
import { ref, onMounted, defineEmits } from 'vue'
import { adminAPI } from '../../services/api'

const emit = defineEmits(['refresh'])

const sellers = ref([])
const loading = ref(true)
const error = ref(null)

onMounted(async () => {
  await loadSellers()
})

const loadSellers = async () => {
  loading.value = true
  try {
    const response = await adminAPI.getPendingSellers()
    sellers.value = response.data || []
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load pending sellers'
  } finally {
    loading.value = false
  }
}

const approveSeller = async (shopId) => {
  try {
    await adminAPI.approveSeller(shopId)
    await loadSellers()
    emit('refresh')
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to approve seller'
  }
}

const rejectSeller = async (shopId) => {
  if (!confirm('Are you sure you want to reject this seller application? This will delete the shop.')) return
  try {
    await adminAPI.rejectSeller(shopId)
    await loadSellers()
    emit('refresh')
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to reject seller'
  }
}

const formatDate = (date) => new Date(date).toLocaleDateString('en-US', {
  year: 'numeric', month: 'short', day: 'numeric'
})
</script>

<template>
  <div class="admin-sellers">
    <h1>Pending Seller Applications</h1>

    <div v-if="error" class="error-message">{{ error }}</div>

    <div v-if="loading" class="loading">Loading pending sellers...</div>

    <div v-else-if="sellers.length === 0" class="empty-state">
      <i class="bi bi-check-circle"></i>
      <p>No pending seller applications.</p>
    </div>

    <div v-else class="sellers-list">
      <div v-for="seller in sellers" :key="seller.shopId" class="seller-card">
        <div class="seller-header">
          <div class="shop-logo" v-if="seller.logoImageUrl">
            <img :src="seller.logoImageUrl" :alt="seller.shopName" />
          </div>
          <div class="shop-logo placeholder" v-else>
            <i class="bi bi-shop"></i>
          </div>
          <div class="shop-info">
            <h3>{{ seller.shopName }}</h3>
            <p class="application-date">Applied: {{ formatDate(seller.createdAt) }}</p>
          </div>
        </div>

        <div class="shop-description" v-if="seller.description">
          <p>{{ seller.description }}</p>
        </div>

        <div class="seller-info">
          <h4>Seller Information</h4>
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">Name:</span>
              <span>{{ seller.seller.firstName }} {{ seller.seller.lastName }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Email:</span>
              <span>{{ seller.seller.email }}</span>
            </div>
            <div class="info-item" v-if="seller.seller.phone">
              <span class="info-label">Phone:</span>
              <span>{{ seller.seller.phone }}</span>
            </div>
          </div>
        </div>

        <div class="card-actions">
          <button class="reject-btn" @click="rejectSeller(seller.shopId)">
            <i class="bi bi-x-lg"></i> Reject
          </button>
          <button class="approve-btn" @click="approveSeller(seller.shopId)">
            <i class="bi bi-check-lg"></i> Approve
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-sellers h1 {
  font-weight: 400;
  margin-bottom: 1.5rem;
}

.sellers-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.seller-card {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.seller-header {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}

.shop-logo {
  width: 80px;
  height: 80px;
  border-radius: 12px;
  overflow: hidden;
}

.shop-logo img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.shop-logo.placeholder {
  background: var(--light-gray);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  color: var(--secondary-color);
}

.shop-info h3 {
  font-weight: 500;
  margin-bottom: 0.25rem;
}

.application-date {
  font-size: 0.85rem;
  color: var(--secondary-color);
}

.shop-description {
  padding: 1rem;
  background: var(--light-gray);
  border-radius: 8px;
  margin-bottom: 1rem;
}

.shop-description p {
  margin: 0;
  color: var(--secondary-color);
}

.seller-info {
  margin-bottom: 1rem;
}

.seller-info h4 {
  font-weight: 500;
  margin-bottom: 0.75rem;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0.75rem;
}

.info-item {
  display: flex;
  gap: 0.5rem;
}

.info-label {
  font-weight: 500;
  color: var(--secondary-color);
}

.card-actions {
  display: flex;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
}

.approve-btn, .reject-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
}

.approve-btn {
  background: #28a745;
  color: #fff;
  border: none;
}

.approve-btn:hover {
  background: #218838;
}

.reject-btn {
  background: #fff;
  color: #dc3545;
  border: 1px solid #dc3545;
}

.reject-btn:hover {
  background: #ffe6e6;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: #fff;
  border-radius: 12px;
}

.empty-state i {
  font-size: 3rem;
  color: #28a745;
  margin-bottom: 1rem;
}

.empty-state p {
  color: var(--secondary-color);
}
</style>
