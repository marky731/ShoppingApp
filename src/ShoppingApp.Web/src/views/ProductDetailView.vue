<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { productsAPI, favoritesAPI } from '../services/api'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const cartStore = useCartStore()
const authStore = useAuthStore()

const product = ref(null)
const loading = ref(true)
const quantity = ref(1)
const isFavorite = ref(false)
const favoriteLoading = ref(false)

const isAuthenticated = computed(() => authStore.isAuthenticated)

onMounted(async () => {
  await loadProduct()
})

watch(() => route.params.slug, async () => {
  await loadProduct()
})

const loadProduct = async () => {
  loading.value = true
  try {
    const response = await productsAPI.getBySlug(route.params.slug)
    product.value = response.data
    // Check if product is in favorites
    if (isAuthenticated.value && product.value) {
      await checkFavoriteStatus()
    }
  } catch (error) {
    console.error('Failed to load product:', error)
  } finally {
    loading.value = false
  }
}

const checkFavoriteStatus = async () => {
  try {
    const response = await favoritesAPI.check(product.value.productId)
    isFavorite.value = response.data.isFavorite
  } catch (error) {
    console.error('Failed to check favorite status:', error)
  }
}

const toggleFavorite = async () => {
  if (!isAuthenticated.value) {
    // Could redirect to login or show message
    return
  }

  favoriteLoading.value = true
  try {
    if (isFavorite.value) {
      await favoritesAPI.remove(product.value.productId)
      isFavorite.value = false
    } else {
      await favoritesAPI.add(product.value.productId)
      isFavorite.value = true
    }
  } catch (error) {
    console.error('Failed to toggle favorite:', error)
  } finally {
    favoriteLoading.value = false
  }
}

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
          <div class="product-image-wrapper">
            <img
              :src="product.mainImageUrl || '/placeholder.png'"
              :alt="product.productName"
              class="img-fluid"
              style="max-height: 500px; object-fit: contain; width: 100%;"
            />
            <button
              v-if="isAuthenticated"
              class="favorite-btn-detail"
              :class="{ active: isFavorite }"
              @click="toggleFavorite"
              :disabled="favoriteLoading"
            >
              <i :class="isFavorite ? 'bi bi-heart-fill' : 'bi bi-heart'"></i>
            </button>
          </div>
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

          <div style="display: flex; gap: 1rem; align-items: center;">
            <button class="add-to-cart-btn" style="padding: 1rem 2rem;" @click="addToCart">
              Add to Cart
            </button>
            <button
              v-if="isAuthenticated"
              class="favorite-btn-inline"
              :class="{ active: isFavorite }"
              @click="toggleFavorite"
              :disabled="favoriteLoading"
            >
              <i :class="isFavorite ? 'bi bi-heart-fill' : 'bi bi-heart'"></i>
              {{ isFavorite ? 'In Favorites' : 'Add to Favorites' }}
            </button>
          </div>

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

<style scoped>
.product-image-wrapper {
  position: relative;
}

.favorite-btn-detail {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: #fff;
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  font-size: 1.2rem;
  transition: all 0.2s;
}

.favorite-btn-detail:hover {
  background: #f5f5f5;
}

.favorite-btn-detail.active {
  color: #ff4444;
}

.favorite-btn-detail:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.favorite-btn-inline {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: none;
  border: 1px solid #e0e0e0;
  padding: 1rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.favorite-btn-inline:hover {
  border-color: #000;
}

.favorite-btn-inline.active {
  color: #ff4444;
  border-color: #ff4444;
}

.favorite-btn-inline:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
