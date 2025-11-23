<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { productsAPI } from '../services/api'
import { useCartStore } from '../stores/cart'

const route = useRoute()
const cartStore = useCartStore()

const product = ref(null)
const loading = ref(true)
const quantity = ref(1)

onMounted(async () => {
  try {
    const response = await productsAPI.getBySlug(route.params.slug)
    product.value = response.data
  } catch (error) {
    console.error('Failed to load product:', error)
  } finally {
    loading.value = false
  }
})

const addToCart = () => {
  if (product.value) {
    cartStore.addItem(product.value, quantity.value)
  }
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}
</script>

<template>
  <div class="page-container">
    <div v-if="loading" class="loading">
      Loading product...
    </div>

    <div v-else-if="product" class="product-detail">
      <div class="row">
        <div class="col-md-6">
          <img
            :src="product.mainImageUrl || '/placeholder.png'"
            :alt="product.productName"
            class="img-fluid"
            style="max-height: 500px; object-fit: contain; width: 100%;"
          />
        </div>
        <div class="col-md-6">
          <h1 style="font-weight: 300; margin-bottom: 1rem;">{{ product.productName }}</h1>

          <p style="font-size: 1.5rem; margin-bottom: 1rem;">
            {{ formatPrice(product.price) }}
          </p>

          <div v-if="product.brand" style="margin-bottom: 0.5rem;">
            <strong>Brand:</strong> {{ product.brand }}
          </div>

          <div v-if="product.shop" style="margin-bottom: 1rem; color: #666;">
            Sold by: {{ product.shop.shopName }}
          </div>

          <p style="margin-bottom: 1.5rem; line-height: 1.6;">
            {{ product.description }}
          </p>

          <div style="display: flex; gap: 1rem; align-items: center; margin-bottom: 1rem;">
            <label>Quantity:</label>
            <input
              v-model.number="quantity"
              type="number"
              min="1"
              :max="product.stockQuantity"
              style="width: 60px; padding: 0.5rem; border: 1px solid #e0e0e0; border-radius: 4px;"
            />
          </div>

          <button class="add-to-cart-btn" style="padding: 1rem 2rem;" @click="addToCart">
            Add to Cart
          </button>

          <div v-if="product.stockQuantity < 10" style="margin-top: 1rem; color: #cc0000;">
            Only {{ product.stockQuantity }} left in stock!
          </div>
        </div>
      </div>
    </div>

    <div v-else class="loading">
      Product not found.
    </div>
  </div>
</template>
