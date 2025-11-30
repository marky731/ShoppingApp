<script setup>
import { ref, onMounted } from 'vue'
import { sellerAPI } from '../../services/api'

const orders = ref([])
const loading = ref(true)
const error = ref(null)
const page = ref(1)
const totalPages = ref(1)
const statusFilter = ref('')
const selectedOrder = ref(null)
const showModal = ref(false)

const statuses = ['pending', 'processing', 'shipped', 'delivered', 'cancelled']

onMounted(async () => {
  await loadOrders()
})

const loadOrders = async () => {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: 10 }
    if (statusFilter.value) params.status = statusFilter.value
    const response = await sellerAPI.getOrders(params)
    orders.value = response.data.items || []
    totalPages.value = response.data.totalPages || 1
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load orders'
  } finally {
    loading.value = false
  }
}

const viewOrder = async (orderId) => {
  try {
    const response = await sellerAPI.getOrder(orderId)
    selectedOrder.value = response.data
    showModal.value = true
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load order details'
  }
}

const updateStatus = async (orderId, newStatus) => {
  try {
    await sellerAPI.updateOrderStatus(orderId, newStatus)
    await loadOrders()
    if (selectedOrder.value && selectedOrder.value.shopOrderId === orderId) {
      selectedOrder.value.status = newStatus
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to update status'
  }
}

const updateTracking = async (orderId) => {
  const tracking = prompt('Enter tracking number:')
  if (!tracking) return

  try {
    await sellerAPI.updateOrderTracking(orderId, tracking)
    await loadOrders()
    if (selectedOrder.value && selectedOrder.value.shopOrderId === orderId) {
      selectedOrder.value.trackingNumber = tracking
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to update tracking'
  }
}

const formatPrice = (price) => `$${price?.toFixed(2) || '0.00'}`
const formatDate = (date) => new Date(date).toLocaleDateString('en-US', {
  year: 'numeric', month: 'short', day: 'numeric'
})

const getStatusClass = (status) => {
  const map = { pending: 'pending', processing: 'processing', shipped: 'shipped', delivered: 'delivered', cancelled: 'cancelled' }
  return map[status] || 'pending'
}
</script>

<template>
  <div class="seller-orders">
    <div class="page-header">
      <h1>Orders</h1>
      <select v-model="statusFilter" @change="page = 1; loadOrders()">
        <option value="">All Status</option>
        <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
      </select>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>

    <!-- Order Detail Modal -->
    <div v-if="showModal && selectedOrder" class="modal-overlay" @click.self="showModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Order #{{ selectedOrder.shopOrderId }}</h2>
          <button class="close-btn" @click="showModal = false">&times;</button>
        </div>

        <div class="order-info">
          <div class="info-row">
            <span>Customer:</span>
            <span>{{ selectedOrder.customerName }}</span>
          </div>
          <div class="info-row">
            <span>Date:</span>
            <span>{{ formatDate(selectedOrder.orderDate) }}</span>
          </div>
          <div class="info-row">
            <span>Status:</span>
            <span :class="['status-badge', getStatusClass(selectedOrder.status)]">{{ selectedOrder.status }}</span>
          </div>
          <div class="info-row">
            <span>Tracking:</span>
            <span>{{ selectedOrder.trackingNumber || 'Not set' }}</span>
          </div>
        </div>

        <div class="shipping-address">
          <h4>Shipping Address</h4>
          <p>{{ selectedOrder.shippingAddress?.streetAddress }}</p>
          <p>{{ selectedOrder.shippingAddress?.city }} {{ selectedOrder.shippingAddress?.postalCode }}</p>
          <p>{{ selectedOrder.shippingAddress?.country }}</p>
        </div>

        <div class="order-items">
          <h4>Items</h4>
          <div v-for="item in selectedOrder.items" :key="item.productId" class="order-item">
            <img :src="item.mainImageUrl || '/placeholder.png'" :alt="item.productName" />
            <div class="item-info">
              <div class="item-name">{{ item.productName }}</div>
              <div class="item-qty">Qty: {{ item.quantity }} x {{ formatPrice(item.priceAtPurchase) }}</div>
            </div>
            <div class="item-total">{{ formatPrice(item.subtotal) }}</div>
          </div>
        </div>

        <div class="order-total">
          <span>Total:</span>
          <span>{{ formatPrice(selectedOrder.total) }}</span>
        </div>

        <div class="modal-actions">
          <select v-model="selectedOrder.status" @change="updateStatus(selectedOrder.shopOrderId, selectedOrder.status)">
            <option v-for="s in statuses" :key="s" :value="s">{{ s }}</option>
          </select>
          <button class="tracking-btn" @click="updateTracking(selectedOrder.shopOrderId)">
            <i class="bi bi-truck"></i> Add Tracking
          </button>
        </div>
      </div>
    </div>

    <!-- Orders Table -->
    <div v-if="loading" class="loading">Loading orders...</div>

    <div v-else-if="orders.length === 0" class="empty-state">
      <i class="bi bi-bag"></i>
      <p>No orders yet.</p>
    </div>

    <table v-else class="orders-table">
      <thead>
        <tr>
          <th>Order ID</th>
          <th>Customer</th>
          <th>Date</th>
          <th>Total</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="order in orders" :key="order.shopOrderId">
          <td>#{{ order.shopOrderId }}</td>
          <td>{{ order.customerName }}</td>
          <td>{{ formatDate(order.orderDate) }}</td>
          <td>{{ formatPrice(order.total) }}</td>
          <td>
            <span :class="['status-badge', getStatusClass(order.status)]">{{ order.status }}</span>
          </td>
          <td>
            <button class="action-btn" @click="viewOrder(order.shopOrderId)" title="View">
              <i class="bi bi-eye"></i>
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="pagination">
      <button @click="page--; loadOrders()" :disabled="page === 1">Previous</button>
      <span>Page {{ page }} of {{ totalPages }}</span>
      <button @click="page++; loadOrders()" :disabled="page === totalPages">Next</button>
    </div>
  </div>
</template>

<style scoped>
.seller-orders h1 {
  font-weight: 400;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.page-header select {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}

.orders-table {
  width: 100%;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  border-collapse: collapse;
}

.orders-table th, .orders-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.orders-table th {
  background: var(--light-gray);
  font-weight: 500;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  text-transform: capitalize;
}

.status-badge.pending { background: #fff3cd; color: #856404; }
.status-badge.processing { background: #cce5ff; color: #004085; }
.status-badge.shipped { background: #d4edda; color: #155724; }
.status-badge.delivered { background: #d1e7dd; color: #0f5132; }
.status-badge.cancelled { background: #f8d7da; color: #721c24; }

.action-btn {
  background: none;
  border: 1px solid var(--border-color);
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
}

.action-btn:hover {
  background: var(--light-gray);
}

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
  max-width: 600px;
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
}

.order-info {
  background: var(--light-gray);
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
}

.shipping-address {
  margin-bottom: 1rem;
}

.shipping-address h4 {
  margin-bottom: 0.5rem;
}

.shipping-address p {
  margin: 0.25rem 0;
  color: var(--secondary-color);
}

.order-items h4 {
  margin-bottom: 0.5rem;
}

.order-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--border-color);
}

.order-item img {
  width: 50px;
  height: 50px;
  object-fit: cover;
  border-radius: 8px;
}

.item-info {
  flex: 1;
}

.item-name {
  font-weight: 500;
}

.item-qty {
  font-size: 0.85rem;
  color: var(--secondary-color);
}

.order-total {
  display: flex;
  justify-content: space-between;
  font-weight: 600;
  font-size: 1.1rem;
  padding: 1rem 0;
  border-top: 2px solid var(--border-color);
  margin-top: 1rem;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.modal-actions select {
  flex: 1;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}

.tracking-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  background: var(--primary-color);
  color: #fff;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: #fff;
  border-radius: 12px;
}

.empty-state i {
  font-size: 3rem;
  color: var(--secondary-color);
  margin-bottom: 1rem;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 1.5rem;
}

.pagination button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color);
  background: #fff;
  border-radius: 6px;
  cursor: pointer;
}
</style>
