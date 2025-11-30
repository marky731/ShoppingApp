<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useCartStore } from '../stores/cart'
import { favoritesAPI } from '../services/api'

const router = useRouter()
const authStore = useAuthStore()
const cartStore = useCartStore()

const favorites = ref([])
const loading = ref(false)
const error = ref(null)
const removingIds = ref(new Set())

const isAuthenticated = computed(() => authStore.isAuthenticated)

onMounted(async () => {
  if (!isAuthenticated.value) {
    router.push('/login')
    return
  }
  await fetchFavorites()
})

const fetchFavorites = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await favoritesAPI.get()
    favorites.value = response.data.items || []
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load favorites'
  } finally {
    loading.value = false
  }
}

const removeFromFavorites = async (productId) => {
  removingIds.value.add(productId)
  try {
    await favoritesAPI.remove(productId)
    favorites.value = favorites.value.filter(f => f.productId !== productId)
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to remove from favorites'
  } finally {
    removingIds.value.delete(productId)
  }
}

const addToCart = async (item) => {
  const success = await cartStore.addItem({
    productId: item.productId,
    productName: item.productName,
    price: item.price,
    mainImageUrl: item.mainImageUrl
  })
  if (success) {
    // Could show a toast notification here
  }
}

const viewProduct = (slug) => {
  router.push(`/products/${slug}`)
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}
</script>

<template>
  <div class="favorites-container">
    <h1 class="auth-title">My Favorites</h1>

    <div v-if="!isAuthenticated" class="loading">
      Please <router-link to="/login">login</router-link> to view your favorites.
    </div>

    <div v-else-if="error" class="error-message">
      {{ error }}
    </div>

    <div v-else-if="loading" class="loading">
      Loading favorites...
    </div>

    <div v-else-if="favorites.length === 0" class="loading">
      You haven't added any favorites yet.
      <router-link to="/products">Browse products</router-link>
    </div>

    <div v-else class="favorites-grid">
      <div
        v-for="item in favorites"
        :key="item.productId"
        class="favorite-card"
      >
        <div class="favorite-image-container" @click="viewProduct(item.slug)">
          <img
            :src="item.mainImageUrl || '/placeholder.png'"
            :alt="item.productName"
            class="favorite-image"
          />
          <button
            class="remove-btn"
            @click.stop="removeFromFavorites(item.productId)"
            :disabled="removingIds.has(item.productId)"
          >
            <i class="bi bi-x-lg"></i>
          </button>
        </div>
        <div class="favorite-info">
          <div class="favorite-name" @click="viewProduct(item.slug)">
            {{ item.productName }}
          </div>
          <div class="favorite-price">{{ formatPrice(item.price) }}</div>
          <div v-if="item.shopName" class="favorite-shop">{{ item.shopName }}</div>
          <button class="add-to-cart-btn" @click="addToCart(item)">
            Add to Cart
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.favorites-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.favorites-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1.5rem;
}

.favorite-card {
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  transition: box-shadow 0.3s;
}

.favorite-card:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12);
}

.favorite-image-container {
  position: relative;
  padding-top: 100%;
  background: var(--light-gray);
  cursor: pointer;
}

.favorite-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.remove-btn {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  background: #fff;
  border: none;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
  color: #cc0000;
}

.remove-btn:hover {
  background: #ffe6e6;
}

.remove-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.favorite-info {
  padding: 1rem;
  text-align: center;
}

.favorite-name {
  font-size: 0.95rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
  color: var(--primary-color);
  cursor: pointer;
}

.favorite-name:hover {
  text-decoration: underline;
}

.favorite-price {
  font-size: 0.9rem;
  color: var(--secondary-color);
  margin-bottom: 0.5rem;
}

.favorite-shop {
  font-size: 0.8rem;
  color: var(--secondary-color);
  margin-bottom: 1rem;
}

.add-to-cart-btn {
  background: var(--primary-color);
  color: #fff;
  border: none;
  padding: 0.5rem 1.5rem;
  border-radius: 20px;
  font-size: 0.85rem;
  cursor: pointer;
  transition: background 0.2s;
}

.add-to-cart-btn:hover {
  background: var(--secondary-color);
}
</style>
