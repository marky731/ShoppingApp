<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { ordersAPI } from '../services/api'

const router = useRouter()
const authStore = useAuthStore()

const orders = ref([])
const loading = ref(false)
const error = ref(null)

const isAuthenticated = computed(() => authStore.isAuthenticated)

onMounted(async () => {
  if (!isAuthenticated.value) {
    router.push('/login')
    return
  }
  await fetchOrders()
})

const fetchOrders = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await ordersAPI.getAll()
    orders.value = response.data
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load orders'
  } finally {
    loading.value = false
  }
}

const viewOrder = (orderId) => {
  router.push(`/orders/${orderId}`)
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const getStatusClass = (status) => {
  const statusMap = {
    'Pending': 'status-pending',
    'Processing': 'status-processing',
    'Shipped': 'status-shipped',
    'Delivered': 'status-delivered',
    'Cancelled': 'status-cancelled'
  }
  return statusMap[status] || 'status-pending'
}
</script>

<template>
  <div class="orders-container">
    <h1 class="auth-title">My Orders</h1>

    <div v-if="error" class="error-message">
      {{ error }}
    </div>

    <div v-if="loading" class="loading">
      Loading orders...
    </div>

    <div v-else-if="orders.length === 0" class="loading">
      You haven't placed any orders yet.
      <router-link to="/products">Start shopping</router-link>
    </div>

    <div v-else class="orders-list">
      <div
        v-for="order in orders"
        :key="order.orderId"
        class="order-card"
        @click="viewOrder(order.orderId)"
      >
        <div class="order-header">
          <div class="order-id">Order #{{ order.orderId }}</div>
          <span :class="['order-status', getStatusClass(order.status)]">
            {{ order.status }}
          </span>
        </div>
        <div class="order-info">
          <div class="order-date">{{ formatDate(order.createdAt) }}</div>
          <div class="order-total">{{ formatPrice(order.totalAmount) }}</div>
        </div>
        <div class="order-items-preview">
          {{ order.itemCount }} item{{ order.itemCount !== 1 ? 's' : '' }}
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.orders-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.orders-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.order-card {
  background: #fff;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 1.25rem;
  cursor: pointer;
  transition: box-shadow 0.2s, border-color 0.2s;
}

.order-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  border-color: var(--primary-color);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.order-id {
  font-weight: 600;
  font-size: 1rem;
}

.order-status {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 500;
}

.status-pending {
  background: #fff3cd;
  color: #856404;
}

.status-processing {
  background: #cce5ff;
  color: #004085;
}

.status-shipped {
  background: #d4edda;
  color: #155724;
}

.status-delivered {
  background: #d1e7dd;
  color: #0f5132;
}

.status-cancelled {
  background: #f8d7da;
  color: #721c24;
}

.order-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.order-date {
  color: var(--secondary-color);
  font-size: 0.9rem;
}

.order-total {
  font-weight: 600;
  font-size: 1.1rem;
}

.order-items-preview {
  color: var(--secondary-color);
  font-size: 0.85rem;
}
</style>
