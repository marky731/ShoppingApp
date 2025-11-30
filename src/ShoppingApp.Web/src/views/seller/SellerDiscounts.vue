<script setup>
import { ref, onMounted } from 'vue'
import { sellerAPI } from '../../services/api'

const discounts = ref([])
const loading = ref(true)
const error = ref(null)
const showForm = ref(false)
const editingDiscount = ref(null)

const discountForm = ref({
  code: '',
  discountType: 'percentage',
  discountValue: 0,
  minimumOrderAmount: 0,
  usageLimit: null,
  expiresAt: null
})

onMounted(async () => {
  await loadDiscounts()
})

const loadDiscounts = async () => {
  loading.value = true
  try {
    const response = await sellerAPI.getDiscounts()
    discounts.value = response.data || []
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load discounts'
  } finally {
    loading.value = false
  }
}

const openAddForm = () => {
  editingDiscount.value = null
  discountForm.value = {
    code: '',
    discountType: 'percentage',
    discountValue: 0,
    minimumOrderAmount: 0,
    usageLimit: null,
    expiresAt: null
  }
  showForm.value = true
}

const openEditForm = (discount) => {
  editingDiscount.value = discount
  discountForm.value = {
    code: discount.code,
    discountType: discount.discountType,
    discountValue: discount.discountValue,
    minimumOrderAmount: discount.minimumOrderAmount || 0,
    usageLimit: discount.usageLimit,
    expiresAt: discount.expiresAt ? discount.expiresAt.split('T')[0] : null
  }
  showForm.value = true
}

const saveDiscount = async () => {
  if (!discountForm.value.code || !discountForm.value.discountValue) {
    error.value = 'Code and discount value are required'
    return
  }

  loading.value = true
  error.value = null
  try {
    const data = { ...discountForm.value }
    if (data.expiresAt) {
      data.expiresAt = new Date(data.expiresAt).toISOString()
    }

    if (editingDiscount.value) {
      await sellerAPI.updateDiscount(editingDiscount.value.discountId, data)
    } else {
      await sellerAPI.createDiscount(data)
    }
    showForm.value = false
    await loadDiscounts()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save discount'
  } finally {
    loading.value = false
  }
}

const deleteDiscount = async (discountId) => {
  if (!confirm('Are you sure you want to delete this discount?')) return

  try {
    await sellerAPI.deleteDiscount(discountId)
    await loadDiscounts()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to delete discount'
  }
}

const formatDate = (date) => date ? new Date(date).toLocaleDateString() : 'No expiry'
const formatValue = (discount) => {
  return discount.discountType === 'percentage'
    ? `${discount.discountValue}%`
    : `$${discount.discountValue.toFixed(2)}`
}
</script>

<template>
  <div class="seller-discounts">
    <div class="page-header">
      <h1>Discounts</h1>
      <button class="add-btn" @click="openAddForm">
        <i class="bi bi-plus-lg"></i> Add Discount
      </button>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>

    <!-- Discount Form Modal -->
    <div v-if="showForm" class="modal-overlay" @click.self="showForm = false">
      <div class="modal-content">
        <h2>{{ editingDiscount ? 'Edit Discount' : 'Add Discount' }}</h2>

        <div class="form-group">
          <label>Discount Code *</label>
          <input v-model="discountForm.code" type="text" placeholder="e.g. SAVE20" />
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Type</label>
            <select v-model="discountForm.discountType">
              <option value="percentage">Percentage (%)</option>
              <option value="fixed">Fixed Amount ($)</option>
            </select>
          </div>

          <div class="form-group">
            <label>Value *</label>
            <input v-model.number="discountForm.discountValue" type="number" min="0" step="0.01" />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Minimum Order Amount</label>
            <input v-model.number="discountForm.minimumOrderAmount" type="number" min="0" step="0.01" />
          </div>

          <div class="form-group">
            <label>Usage Limit</label>
            <input v-model.number="discountForm.usageLimit" type="number" min="0" placeholder="Unlimited" />
          </div>
        </div>

        <div class="form-group">
          <label>Expires At</label>
          <input v-model="discountForm.expiresAt" type="date" />
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="showForm = false">Cancel</button>
          <button class="save-btn" @click="saveDiscount" :disabled="loading">
            {{ loading ? 'Saving...' : 'Save Discount' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Discounts Table -->
    <div v-if="loading && !showForm" class="loading">Loading discounts...</div>

    <div v-else-if="discounts.length === 0" class="empty-state">
      <i class="bi bi-tag"></i>
      <p>No discounts yet. Create your first discount code!</p>
    </div>

    <table v-else class="discounts-table">
      <thead>
        <tr>
          <th>Code</th>
          <th>Discount</th>
          <th>Min. Order</th>
          <th>Usage</th>
          <th>Expires</th>
          <th>Status</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="discount in discounts" :key="discount.discountId">
          <td><code>{{ discount.code }}</code></td>
          <td>{{ formatValue(discount) }}</td>
          <td>${{ discount.minimumOrderAmount?.toFixed(2) || '0.00' }}</td>
          <td>{{ discount.usageCount || 0 }} / {{ discount.usageLimit || '∞' }}</td>
          <td>{{ formatDate(discount.expiresAt) }}</td>
          <td>
            <span :class="['status-badge', discount.isActive ? 'active' : 'inactive']">
              {{ discount.isActive ? 'Active' : 'Inactive' }}
            </span>
          </td>
          <td>
            <button class="action-btn" @click="openEditForm(discount)" title="Edit">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="action-btn delete" @click="deleteDiscount(discount.discountId)" title="Delete">
              <i class="bi bi-trash"></i>
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.seller-discounts h1 {
  font-weight: 400;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.add-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--primary-color);
  color: #fff;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
}

.add-btn:hover {
  background: var(--secondary-color);
}

.discounts-table {
  width: 100%;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  border-collapse: collapse;
}

.discounts-table th, .discounts-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.discounts-table th {
  background: var(--light-gray);
  font-weight: 500;
}

.discounts-table code {
  background: var(--light-gray);
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-family: monospace;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
}

.status-badge.active {
  background: #d4edda;
  color: #155724;
}

.status-badge.inactive {
  background: #f8d7da;
  color: #721c24;
}

.action-btn {
  background: none;
  border: 1px solid var(--border-color);
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  margin-right: 0.5rem;
}

.action-btn:hover {
  background: var(--light-gray);
}

.action-btn.delete {
  color: #dc3545;
}

.action-btn.delete:hover {
  background: #ffe6e6;
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
  max-width: 500px;
}

.modal-content h2 {
  margin-bottom: 1.5rem;
  font-weight: 500;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group input, .form-group select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
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
</style>
