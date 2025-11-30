<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { ordersAPI } from '../services/api'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const order = ref(null)
const loading = ref(false)
const cancelling = ref(false)
const error = ref(null)

const isAuthenticated = computed(() => authStore.isAuthenticated)
const canCancel = computed(() => order.value && order.value.status === 'Pending')

onMounted(async () => {
  if (!isAuthenticated.value) {
    router.push('/login')
    return
  }
  await fetchOrder()
})

const fetchOrder = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await ordersAPI.getById(route.params.id)
    order.value = response.data
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load order'
  } finally {
    loading.value = false
  }
}

const cancelOrder = async () => {
  if (!confirm('Are you sure you want to cancel this order?')) return

  cancelling.value = true
  error.value = null
  try {
    await ordersAPI.cancel(order.value.orderId)
    await fetchOrder()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to cancel order'
  } finally {
    cancelling.value = false
  }
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
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
  <div class="order-detail-container">
    <button class="back-btn" @click="router.push('/orders')">
      <i class="bi bi-arrow-left"></i> Back to Orders
    </button>

    <div v-if="error" class="error-message">
      {{ error }}
    </div>

    <div v-if="loading" class="loading">
      Loading order details...
    </div>

    <div v-else-if="order" class="order-detail">
      <div class="order-header">
        <div>
          <h1 class="order-title">Order #{{ order.orderId }}</h1>
          <p class="order-date">Placed on {{ formatDate(order.createdAt) }}</p>
        </div>
        <span :class="['order-status', getStatusClass(order.status)]">
          {{ order.status }}
        </span>
      </div>

      <!-- Shipping Address -->
      <div class="detail-section">
        <h2 class="section-subtitle">Shipping Address</h2>
        <div class="address-info">
          <div class="address-label-tag">{{ order.shippingAddress.addressLabel }}</div>
          <div>{{ order.shippingAddress.streetAddress }}</div>
          <div>{{ order.shippingAddress.city }} {{ order.shippingAddress.postalCode }}</div>
          <div>{{ order.shippingAddress.country }}</div>
        </div>
      </div>

      <!-- Shop Orders -->
      <div v-for="shopOrder in order.shopOrders" :key="shopOrder.shopOrderId" class="detail-section">
        <h2 class="section-subtitle">
          {{ shopOrder.shopName }}
          <span :class="['shop-status', getStatusClass(shopOrder.status)]">
            {{ shopOrder.status }}
          </span>
        </h2>

        <div v-if="shopOrder.trackingNumber" class="tracking-info">
          Tracking: {{ shopOrder.trackingNumber }}
        </div>

        <div class="order-items">
          <div v-for="item in shopOrder.items" :key="item.productId" class="order-item">
            <img
              :src="item.mainImageUrl || '/placeholder.png'"
              :alt="item.productName"
              class="item-image"
            />
            <div class="item-info">
              <div class="item-name">{{ item.productName }}</div>
              <div class="item-price">{{ formatPrice(item.priceAtPurchase) }} x {{ item.quantity }}</div>
            </div>
            <div class="item-total">{{ formatPrice(item.priceAtPurchase * item.quantity) }}</div>
          </div>
        </div>

        <div class="shop-subtotal">
          Subtotal: {{ formatPrice(shopOrder.subtotal) }}
        </div>
      </div>

      <!-- Order Total -->
      <div class="detail-section total-section">
        <div class="total-row">
          <span>Subtotal</span>
          <span>{{ formatPrice(order.subtotal) }}</span>
        </div>
        <div v-if="order.discountAmount > 0" class="total-row discount">
          <span>Discount ({{ order.discountCode }})</span>
          <span>-{{ formatPrice(order.discountAmount) }}</span>
        </div>
        <div class="total-row final">
          <span>Total</span>
          <span>{{ formatPrice(order.totalAmount) }}</span>
        </div>
      </div>

      <!-- Cancel Button -->
      <button
        v-if="canCancel"
        class="cancel-btn"
        @click="cancelOrder"
        :disabled="cancelling"
      >
        {{ cancelling ? 'Cancelling...' : 'Cancel Order' }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.order-detail-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.back-btn {
  background: none;
  border: none;
  color: var(--secondary-color);
  cursor: pointer;
  font-size: 0.9rem;
  margin-bottom: 1.5rem;
  padding: 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.back-btn:hover {
  color: var(--primary-color);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
}

.order-title {
  font-size: 1.5rem;
  font-weight: 500;
  margin-bottom: 0.25rem;
}

.order-date {
  color: var(--secondary-color);
  font-size: 0.9rem;
}

.order-status {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: 500;
}

.status-pending { background: #fff3cd; color: #856404; }
.status-processing { background: #cce5ff; color: #004085; }
.status-shipped { background: #d4edda; color: #155724; }
.status-delivered { background: #d1e7dd; color: #0f5132; }
.status-cancelled { background: #f8d7da; color: #721c24; }

.detail-section {
  background: #fff;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 1rem;
}

.section-subtitle {
  font-size: 1rem;
  font-weight: 500;
  margin-bottom: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.shop-status {
  padding: 0.25rem 0.5rem;
  border-radius: 10px;
  font-size: 0.75rem;
}

.address-info {
  font-size: 0.9rem;
  line-height: 1.6;
  color: var(--secondary-color);
}

.address-label-tag {
  font-weight: 600;
  color: var(--primary-color);
}

.tracking-info {
  background: var(--light-gray);
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-size: 0.85rem;
  margin-bottom: 1rem;
}

.order-items {
  border-top: 1px solid var(--border-color);
}

.order-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem 0;
  border-bottom: 1px solid var(--light-gray);
}

.order-item:last-child {
  border-bottom: none;
}

.item-image {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 4px;
}

.item-info {
  flex: 1;
}

.item-name {
  font-weight: 500;
  margin-bottom: 0.25rem;
}

.item-price {
  font-size: 0.85rem;
  color: var(--secondary-color);
}

.item-total {
  font-weight: 500;
}

.shop-subtotal {
  text-align: right;
  padding-top: 0.75rem;
  font-weight: 500;
  color: var(--secondary-color);
}

.total-section {
  background: var(--light-gray);
}

.total-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  font-size: 0.95rem;
}

.total-row.discount {
  color: #28a745;
}

.total-row.final {
  font-size: 1.2rem;
  font-weight: 600;
  border-top: 2px solid var(--border-color);
  padding-top: 1rem;
  margin-top: 0.5rem;
}

.cancel-btn {
  width: 100%;
  background: #dc3545;
  color: #fff;
  border: none;
  padding: 1rem;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  margin-top: 1rem;
}

.cancel-btn:hover:not(:disabled) {
  background: #c82333;
}

.cancel-btn:disabled {
  background: #e4606d;
  cursor: not-allowed;
}
</style>
