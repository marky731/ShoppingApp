<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'
import { addressAPI, ordersAPI } from '../services/api'

const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()

const addresses = ref([])
const selectedAddressId = ref(null)
const discountCode = ref('')
const loading = ref(false)
const addressLoading = ref(false)
const error = ref(null)
const showAddressForm = ref(false)

const newAddress = ref({
  addressLabel: '',
  streetAddress: '',
  city: '',
  postalCode: '',
  country: ''
})

const cartItems = computed(() => cartStore.cartItems)
const totalPrice = computed(() => cartStore.totalPrice)
const isAuthenticated = computed(() => authStore.isAuthenticated)

onMounted(async () => {
  if (!isAuthenticated.value) {
    router.push('/login')
    return
  }
  await fetchAddresses()
  await cartStore.fetchCart()
})

const fetchAddresses = async () => {
  addressLoading.value = true
  try {
    const response = await addressAPI.getAll()
    addresses.value = response.data
    // Auto-select default address
    const defaultAddr = addresses.value.find(a => a.isDefault)
    if (defaultAddr) {
      selectedAddressId.value = defaultAddr.addressId
    } else if (addresses.value.length > 0) {
      selectedAddressId.value = addresses.value[0].addressId
    }
  } catch (err) {
    error.value = 'Failed to load addresses'
  } finally {
    addressLoading.value = false
  }
}

const saveAddress = async () => {
  if (!newAddress.value.addressLabel || !newAddress.value.streetAddress || !newAddress.value.city || !newAddress.value.country) {
    error.value = 'Please fill in required fields'
    return
  }

  addressLoading.value = true
  error.value = null
  try {
    await addressAPI.create(newAddress.value)
    await fetchAddresses()
    showAddressForm.value = false
    newAddress.value = {
      addressLabel: '',
      streetAddress: '',
      city: '',
      postalCode: '',
      country: ''
    }
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save address'
  } finally {
    addressLoading.value = false
  }
}

const placeOrder = async () => {
  if (!selectedAddressId.value) {
    error.value = 'Please select a shipping address'
    return
  }

  if (cartItems.value.length === 0) {
    error.value = 'Your cart is empty'
    return
  }

  loading.value = true
  error.value = null
  try {
    const response = await ordersAPI.create(
      selectedAddressId.value,
      discountCode.value || null
    )
    // Clear cart after successful order
    await cartStore.fetchCart()
    router.push(`/orders/${response.data.orderId}`)
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to place order'
  } finally {
    loading.value = false
  }
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}
</script>

<template>
  <div class="checkout-container">
    <h1 class="auth-title">Checkout</h1>

    <div v-if="error" class="error-message">
      {{ error }}
    </div>

    <div v-if="cartItems.length === 0 && !loading" class="loading">
      Your cart is empty. <router-link to="/products">Continue shopping</router-link>
    </div>

    <div v-else class="checkout-content">
      <!-- Order Summary -->
      <div class="checkout-section">
        <h2 class="section-subtitle">Order Summary</h2>
        <div class="order-items">
          <div v-for="item in cartItems" :key="item.productId" class="order-item">
            <img
              :src="item.mainImageUrl || '/placeholder.png'"
              :alt="item.productName"
              class="order-item-image"
            />
            <div class="order-item-info">
              <div class="order-item-name">{{ item.productName }}</div>
              <div class="order-item-qty">Qty: {{ item.quantity }}</div>
            </div>
            <div class="order-item-price">{{ formatPrice(item.price * item.quantity) }}</div>
          </div>
        </div>
        <div class="order-total">
          <span>Total:</span>
          <span>{{ formatPrice(totalPrice) }}</span>
        </div>
      </div>

      <!-- Shipping Address -->
      <div class="checkout-section">
        <h2 class="section-subtitle">Shipping Address</h2>

        <div v-if="addressLoading" class="loading">Loading addresses...</div>

        <div v-else-if="addresses.length === 0 && !showAddressForm" class="no-addresses">
          <p>No saved addresses. Please add one.</p>
          <button class="add-address-btn" @click="showAddressForm = true">
            Add Address
          </button>
        </div>

        <div v-else-if="!showAddressForm">
          <div class="address-list">
            <label
              v-for="addr in addresses"
              :key="addr.addressId"
              class="address-option"
              :class="{ selected: selectedAddressId === addr.addressId }"
            >
              <input
                type="radio"
                :value="addr.addressId"
                v-model="selectedAddressId"
              />
              <div class="address-details">
                <div class="address-label-tag">{{ addr.addressLabel }}</div>
                <div>{{ addr.streetAddress }}</div>
                <div>{{ addr.city }} {{ addr.postalCode }}</div>
                <div>{{ addr.country }}</div>
              </div>
            </label>
          </div>
          <button class="add-address-btn" @click="showAddressForm = true">
            Add New Address
          </button>
        </div>

        <!-- Address Form -->
        <div v-if="showAddressForm" class="address-form">
          <div class="form-group">
            <label>Label * (e.g. Home, Office)</label>
            <input v-model="newAddress.addressLabel" type="text" placeholder="Home" required />
          </div>
          <div class="form-group">
            <label>Street Address *</label>
            <input v-model="newAddress.streetAddress" type="text" required />
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>City *</label>
              <input v-model="newAddress.city" type="text" required />
            </div>
            <div class="form-group">
              <label>Postal Code</label>
              <input v-model="newAddress.postalCode" type="text" />
            </div>
          </div>
          <div class="form-group">
            <label>Country *</label>
            <input v-model="newAddress.country" type="text" required />
          </div>
          <div class="form-actions">
            <button class="cancel-btn" @click="showAddressForm = false">Cancel</button>
            <button class="save-btn" @click="saveAddress" :disabled="addressLoading">
              {{ addressLoading ? 'Saving...' : 'Save Address' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Discount Code -->
      <div class="checkout-section">
        <h2 class="section-subtitle">Discount Code</h2>
        <div class="discount-input">
          <input
            v-model="discountCode"
            type="text"
            placeholder="Enter discount code (optional)"
          />
        </div>
      </div>

      <!-- Place Order Button -->
      <button
        class="place-order-btn"
        @click="placeOrder"
        :disabled="loading || !selectedAddressId || cartItems.length === 0"
      >
        {{ loading ? 'Placing Order...' : 'Place Order' }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.checkout-container {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.checkout-section {
  background: #fff;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
}

.section-subtitle {
  font-size: 1.1rem;
  font-weight: 500;
  margin-bottom: 1rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--border-color);
}

.order-items {
  margin-bottom: 1rem;
}

.order-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--light-gray);
}

.order-item:last-child {
  border-bottom: none;
}

.order-item-image {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 4px;
}

.order-item-info {
  flex: 1;
}

.order-item-name {
  font-weight: 500;
  margin-bottom: 0.25rem;
}

.order-item-qty {
  font-size: 0.85rem;
  color: var(--secondary-color);
}

.order-item-price {
  font-weight: 500;
}

.order-total {
  display: flex;
  justify-content: space-between;
  font-size: 1.2rem;
  font-weight: 600;
  padding-top: 1rem;
  border-top: 2px solid var(--border-color);
}

.address-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.address-option {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  cursor: pointer;
  transition: border-color 0.2s;
}

.address-option:hover {
  border-color: var(--primary-color);
}

.address-option.selected {
  border-color: var(--primary-color);
  background: var(--light-gray);
}

.address-option input[type="radio"] {
  margin-top: 0.25rem;
}

.address-details {
  font-size: 0.9rem;
  line-height: 1.5;
}

.address-label-tag {
  font-weight: 600;
  color: var(--primary-color);
  margin-bottom: 0.25rem;
}

.add-address-btn {
  background: none;
  border: 1px dashed var(--border-color);
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  color: var(--secondary-color);
  width: 100%;
}

.add-address-btn:hover {
  border-color: var(--primary-color);
  color: var(--primary-color);
}

.address-form {
  background: var(--light-gray);
  padding: 1rem;
  border-radius: 4px;
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

.cancel-btn {
  flex: 1;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  background: #fff;
  border-radius: 4px;
  cursor: pointer;
}

.save-btn {
  flex: 1;
  padding: 0.75rem;
  background: var(--primary-color);
  color: #fff;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.save-btn:hover {
  background: var(--secondary-color);
}

.discount-input input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 4px;
}

.place-order-btn {
  width: 100%;
  background: var(--primary-color);
  color: #fff;
  border: none;
  padding: 1rem;
  border-radius: 4px;
  font-size: 1.1rem;
  cursor: pointer;
  transition: background 0.2s;
}

.place-order-btn:hover:not(:disabled) {
  background: var(--secondary-color);
}

.place-order-btn:disabled {
  background: var(--secondary-color);
  cursor: not-allowed;
}

.no-addresses {
  text-align: center;
  padding: 1rem;
}

.no-addresses p {
  margin-bottom: 1rem;
  color: var(--secondary-color);
}
</style>
