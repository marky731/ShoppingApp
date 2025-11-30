<script setup>
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const cartStore = useCartStore()
const authStore = useAuthStore()

const cartItems = computed(() => cartStore.cartItems)
const totalPrice = computed(() => cartStore.totalPrice)
const loading = computed(() => cartStore.loading)
const isAuthenticated = computed(() => authStore.isAuthenticated)

onMounted(() => {
  cartStore.fetchCart()
})

const updateQuantity = async (productId, quantity) => {
  await cartStore.updateQuantity(productId, quantity)
}

const removeItem = async (productId) => {
  await cartStore.removeItem(productId)
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}

const proceedToCheckout = () => {
  if (!isAuthenticated.value) {
    router.push('/login')
    return
  }
  router.push('/checkout')
}
</script>

<template>
  <div class="cart-container">
    <h1 class="auth-title">Shopping Cart</h1>

    <div v-if="loading" class="loading">
      Loading cart...
    </div>

    <div v-else-if="cartItems.length === 0" class="loading">
      Your cart is empty.
    </div>

    <div v-else>
      <div
        v-for="item in cartItems"
        :key="item.productId"
        class="cart-item"
      >
        <img
          :src="item.mainImageUrl || '/placeholder.png'"
          :alt="item.productName"
          class="cart-item-image"
        />
        <div class="cart-item-info">
          <div class="cart-item-name">{{ item.productName }}</div>
          <div class="cart-item-price">{{ formatPrice(item.price) }}</div>
          <div v-if="item.shopName" class="cart-item-shop">{{ item.shopName }}</div>
        </div>
        <div class="cart-item-quantity">
          <button
            class="quantity-btn"
            @click="updateQuantity(item.productId, item.quantity - 1)"
            :disabled="loading"
          >
            -
          </button>
          <span>{{ item.quantity }}</span>
          <button
            class="quantity-btn"
            @click="updateQuantity(item.productId, item.quantity + 1)"
            :disabled="loading"
          >
            +
          </button>
        </div>
        <button
          style="background: none; border: none; cursor: pointer; color: #cc0000;"
          @click="removeItem(item.productId)"
          :disabled="loading"
        >
          <i class="bi bi-x-lg"></i>
        </button>
      </div>

      <div class="cart-total">
        Total: {{ formatPrice(totalPrice) }}
      </div>

      <button class="checkout-btn" @click="proceedToCheckout" :disabled="loading">
        {{ isAuthenticated ? 'Proceed to Checkout' : 'Login to Checkout' }}
      </button>
    </div>
  </div>
</template>
