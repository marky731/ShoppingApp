<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { addressAPI } from '../services/api'

const router = useRouter()
const authStore = useAuthStore()

const user = computed(() => authStore.currentUser)
const isAuthenticated = computed(() => authStore.isAuthenticated)
const isSeller = computed(() => user.value?.role === 'seller')
const isAdmin = computed(() => user.value?.role === 'admin')

const addresses = ref([])
const addressLoading = ref(false)
const showAddressForm = ref(false)
const editingAddress = ref(null)
const error = ref(null)
const success = ref(null)

const newAddress = ref({
  addressLabel: '',
  streetAddress: '',
  city: '',
  postalCode: '',
  country: ''
})

onMounted(async () => {
  if (!isAuthenticated.value) {
    router.push('/login')
    return
  }
  await authStore.fetchUser()
  await fetchAddresses()
})

const fetchAddresses = async () => {
  addressLoading.value = true
  try {
    const response = await addressAPI.getAll()
    addresses.value = response.data
  } catch (err) {
    error.value = 'Failed to load addresses'
  } finally {
    addressLoading.value = false
  }
}

const saveAddress = async () => {
  if (!newAddress.value.addressLabel || !newAddress.value.streetAddress || !newAddress.value.city || !newAddress.value.country) {
    error.value = 'Please fill in all required fields'
    return
  }

  addressLoading.value = true
  error.value = null
  try {
    if (editingAddress.value) {
      await addressAPI.update(editingAddress.value.addressId, newAddress.value)
      success.value = 'Address updated successfully'
    } else {
      await addressAPI.create(newAddress.value)
      success.value = 'Address created successfully'
    }
    await fetchAddresses()
    cancelEdit()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save address'
  } finally {
    addressLoading.value = false
  }
}

const editAddress = (address) => {
  editingAddress.value = address
  newAddress.value = {
    addressLabel: address.addressLabel,
    streetAddress: address.streetAddress,
    city: address.city,
    postalCode: address.postalCode || '',
    country: address.country
  }
  showAddressForm.value = true
}

const deleteAddress = async (addressId) => {
  if (!confirm('Are you sure you want to delete this address?')) return

  try {
    await addressAPI.delete(addressId)
    success.value = 'Address deleted successfully'
    await fetchAddresses()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to delete address'
  }
}

const cancelEdit = () => {
  showAddressForm.value = false
  editingAddress.value = null
  newAddress.value = {
    addressLabel: '',
    streetAddress: '',
    city: '',
    postalCode: '',
    country: ''
  }
}

const handleLogout = () => {
  authStore.logout()
  router.push('/')
}

const clearMessages = () => {
  error.value = null
  success.value = null
}
</script>

<template>
  <div class="profile-container">
    <h1 class="auth-title">My Profile</h1>

    <div v-if="error" class="error-message" @click="clearMessages">
      {{ error }}
    </div>

    <div v-if="success" class="success-message" @click="clearMessages">
      {{ success }}
    </div>

    <!-- User Info Section -->
    <div class="profile-section">
      <h2 class="section-subtitle">Account Information</h2>
      <div v-if="user" class="user-info">
        <div class="info-row">
          <span class="info-label">Name:</span>
          <span class="info-value">{{ user.firstName }} {{ user.lastName }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">Email:</span>
          <span class="info-value">{{ user.email }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">Phone:</span>
          <span class="info-value">{{ user.phone || 'Not set' }}</span>
        </div>
        <div class="info-row">
          <span class="info-label">Account Type:</span>
          <span class="info-value role-badge">{{ user.role }}</span>
        </div>
      </div>
    </div>

    <!-- Quick Links -->
    <div class="profile-section">
      <h2 class="section-subtitle">Quick Links</h2>
      <div class="quick-links">
        <router-link to="/orders" class="quick-link">
          <i class="bi bi-bag"></i>
          <span>My Orders</span>
        </router-link>
        <router-link to="/favorites" class="quick-link">
          <i class="bi bi-heart"></i>
          <span>My Favorites</span>
        </router-link>
        <router-link to="/cart" class="quick-link">
          <i class="bi bi-cart3"></i>
          <span>Shopping Cart</span>
        </router-link>
        <router-link v-if="isSeller" to="/seller" class="quick-link seller-link">
          <i class="bi bi-shop"></i>
          <span>Seller Dashboard</span>
        </router-link>
        <router-link v-if="isAdmin" to="/admin" class="quick-link admin-link">
          <i class="bi bi-shield-check"></i>
          <span>Admin Dashboard</span>
        </router-link>
      </div>
    </div>

    <!-- Addresses Section -->
    <div class="profile-section">
      <div class="section-header">
        <h2 class="section-subtitle">My Addresses</h2>
        <button v-if="!showAddressForm" class="add-btn" @click="showAddressForm = true">
          + Add Address
        </button>
      </div>

      <div v-if="addressLoading && !showAddressForm" class="loading">
        Loading addresses...
      </div>

      <!-- Address Form -->
      <div v-if="showAddressForm" class="address-form">
        <h3>{{ editingAddress ? 'Edit Address' : 'Add New Address' }}</h3>
        <div class="form-group">
          <label>Label * (e.g. Home, Office)</label>
          <input v-model="newAddress.addressLabel" type="text" placeholder="Home" />
        </div>
        <div class="form-group">
          <label>Street Address *</label>
          <input v-model="newAddress.streetAddress" type="text" />
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>City *</label>
            <input v-model="newAddress.city" type="text" />
          </div>
          <div class="form-group">
            <label>Postal Code</label>
            <input v-model="newAddress.postalCode" type="text" />
          </div>
        </div>
        <div class="form-group">
          <label>Country *</label>
          <input v-model="newAddress.country" type="text" />
        </div>
        <div class="form-actions">
          <button class="cancel-btn" @click="cancelEdit">Cancel</button>
          <button class="save-btn" @click="saveAddress" :disabled="addressLoading">
            {{ addressLoading ? 'Saving...' : (editingAddress ? 'Update' : 'Save') }}
          </button>
        </div>
      </div>

      <!-- Address List -->
      <div v-else-if="addresses.length === 0" class="no-addresses">
        No addresses saved yet.
      </div>

      <div v-else class="address-list">
        <div v-for="addr in addresses" :key="addr.addressId" class="address-card">
          <div class="address-content">
            <div class="address-label-tag">{{ addr.addressLabel }}</div>
            <div>{{ addr.streetAddress }}</div>
            <div>{{ addr.city }} {{ addr.postalCode }}</div>
            <div>{{ addr.country }}</div>
          </div>
          <div class="address-actions">
            <button class="edit-btn" @click="editAddress(addr)">
              <i class="bi bi-pencil"></i>
            </button>
            <button class="delete-btn" @click="deleteAddress(addr.addressId)">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Logout -->
    <div class="profile-section">
      <button class="logout-btn" @click="handleLogout">
        <i class="bi bi-box-arrow-right"></i>
        Logout
      </button>
    </div>
  </div>
</template>

<style scoped>
.profile-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.profile-section {
  background: #fff;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.section-subtitle {
  font-size: 1.1rem;
  font-weight: 500;
  margin-bottom: 1rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--border-color);
}

.section-header .section-subtitle {
  margin-bottom: 0;
  padding-bottom: 0;
  border-bottom: none;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.info-row {
  display: flex;
  gap: 1rem;
}

.info-label {
  font-weight: 500;
  min-width: 120px;
  color: var(--secondary-color);
}

.role-badge {
  display: inline-block;
  background: var(--light-gray);
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.85rem;
  text-transform: capitalize;
}

.quick-links {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
}

@media (max-width: 768px) {
  .quick-links {
    grid-template-columns: repeat(2, 1fr);
  }
}

.quick-link {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  background: var(--light-gray);
  border-radius: 8px;
  text-decoration: none;
  color: var(--primary-color);
  transition: background 0.2s;
}

.quick-link:hover {
  background: var(--border-color);
}

.quick-link i {
  font-size: 1.5rem;
}

.add-btn {
  background: var(--primary-color);
  color: #fff;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.add-btn:hover {
  background: var(--secondary-color);
}

.address-form {
  background: var(--light-gray);
  padding: 1.5rem;
  border-radius: 8px;
  margin-top: 1rem;
}

.address-form h3 {
  margin-bottom: 1rem;
  font-weight: 500;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.cancel-btn, .save-btn {
  flex: 1;
  padding: 0.75rem;
  border-radius: 4px;
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

.save-btn:hover {
  background: var(--secondary-color);
}

.address-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 1rem;
}

.address-card {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}

.address-label-tag {
  font-weight: 600;
  color: var(--primary-color);
  margin-bottom: 0.25rem;
}

.address-content {
  font-size: 0.9rem;
  line-height: 1.5;
}

.address-actions {
  display: flex;
  gap: 0.5rem;
}

.edit-btn, .delete-btn {
  width: 32px;
  height: 32px;
  border: 1px solid var(--border-color);
  background: #fff;
  border-radius: 4px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.edit-btn:hover {
  background: var(--light-gray);
}

.delete-btn {
  color: #dc3545;
}

.delete-btn:hover {
  background: #ffe6e6;
}

.no-addresses {
  color: var(--secondary-color);
  text-align: center;
  padding: 1rem;
}

.logout-btn {
  width: 100%;
  background: #dc3545;
  color: #fff;
  border: none;
  padding: 1rem;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.logout-btn:hover {
  background: #c82333;
}

.success-message {
  background: #d4edda;
  color: #155724;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
  cursor: pointer;
}
</style>
