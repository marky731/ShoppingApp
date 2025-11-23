<script setup>
import { RouterLink } from 'vue-router'
import { useCartStore } from '../../stores/cart'

const props = defineProps({
  product: {
    type: Object,
    required: true
  }
})

const cartStore = useCartStore()

const addToCart = () => {
  cartStore.addItem(props.product)
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}
</script>

<template>
  <div class="product-card">
    <RouterLink :to="{ name: 'product-detail', params: { slug: product.slug } }">
      <div class="product-image-container">
        <img
          :src="product.mainImageUrl || '/placeholder.png'"
          :alt="product.productName"
          class="product-image"
        />
        <button class="favorite-btn" @click.prevent>
          ♡
        </button>
      </div>
    </RouterLink>
    <div class="product-info">
      <h3 class="product-name">{{ product.productName }}</h3>
      <p class="product-price">{{ formatPrice(product.price) }}</p>
      <button class="add-to-cart-btn" @click="addToCart">
        Add to Cart
      </button>
    </div>
  </div>
</template>
