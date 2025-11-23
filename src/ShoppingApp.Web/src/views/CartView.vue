<script setup>
import { computed } from 'vue'
import { useCartStore } from '../stores/cart'

const cartStore = useCartStore()

const cartItems = computed(() => cartStore.cartItems)
const totalPrice = computed(() => cartStore.totalPrice)

const updateQuantity = (productId, quantity) => {
  cartStore.updateQuantity(productId, quantity)
}

const removeItem = (productId) => {
  cartStore.removeItem(productId)
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}
</script>

<template>
  <div class="cart-container">
    <h1 class="auth-title">Shopping Cart</h1>

    <div v-if="cartItems.length === 0" class="loading">
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
        </div>
        <div class="cart-item-quantity">
          <button
            class="quantity-btn"
            @click="updateQuantity(item.productId, item.quantity - 1)"
          >
            -
          </button>
          <span>{{ item.quantity }}</span>
          <button
            class="quantity-btn"
            @click="updateQuantity(item.productId, item.quantity + 1)"
          >
            +
          </button>
        </div>
        <button
          style="background: none; border: none; cursor: pointer; color: #cc0000;"
          @click="removeItem(item.productId)"
        >
          ✕
        </button>
      </div>

      <div class="cart-total">
        Total: {{ formatPrice(totalPrice) }}
      </div>

      <button class="checkout-btn">
        Proceed to Checkout
      </button>
    </div>
  </div>
</template>
