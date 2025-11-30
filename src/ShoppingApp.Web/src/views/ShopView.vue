<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { shopsAPI } from '../services/api'
import { useCartStore } from '../stores/cart'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()

const shop = ref(null)
const products = ref([])
const loading = ref(true)
const error = ref(null)
const page = ref(1)
const totalPages = ref(1)
const totalProducts = ref(0)

onMounted(async () => {
  await loadShop()
})

watch(() => route.params.id, async () => {
  page.value = 1
  await loadShop()
})

watch(page, async () => {
  await loadShop()
})

const loadShop = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await shopsAPI.getById(route.params.id, { page: page.value, pageSize: 12 })
    shop.value = response.data.shop
    products.value = response.data.products
    totalPages.value = response.data.totalPages
    totalProducts.value = response.data.totalProducts
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load shop'
  } finally {
    loading.value = false
  }
}

const viewProduct = (slug) => {
  router.push(`/products/${slug}`)
}

const addToCart = async (product) => {
  await cartStore.addItem(product)
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}

const renderStars = (rating) => {
  const fullStars = Math.floor(rating)
  const hasHalf = rating - fullStars >= 0.5
  let stars = ''
  for (let i = 0; i < fullStars; i++) stars += '★'
  if (hasHalf) stars += '½'
  for (let i = stars.length; i < 5; i++) stars += '☆'
  return stars
}
</script>

<template>
  <div class="shop-page">
    <div v-if="loading" class="loading">
      Loading shop...
    </div>

    <div v-else-if="error" class="error-message">
      {{ error }}
    </div>

    <div v-else-if="shop">
      <!-- Shop Header -->
      <div class="shop-header" :style="shop.bannerImageUrl ? { backgroundImage: `url(${shop.bannerImageUrl})` } : {}">
        <div class="shop-header-overlay">
          <div class="shop-info">
            <img
              :src="shop.logoImageUrl || '/placeholder.png'"
              :alt="shop.shopName"
              class="shop-logo"
            />
            <div class="shop-details">
              <h1 class="shop-name">{{ shop.shopName }}</h1>
              <p v-if="shop.description" class="shop-description">{{ shop.description }}</p>
              <div class="shop-stats">
                <span class="shop-rating">
                  <span class="stars">{{ renderStars(shop.averageRating) }}</span>
                  {{ shop.averageRating.toFixed(1) }}
                </span>
                <span class="shop-sales">{{ shop.totalSales }} sales</span>
                <span class="shop-products">{{ shop.productCount }} products</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Products Section -->
      <div class="page-container">
        <h2 class="section-title">Products ({{ totalProducts }})</h2>

        <div v-if="products.length === 0" class="loading">
          This shop has no products yet.
        </div>

        <div v-else class="products-grid">
          <div
            v-for="product in products"
            :key="product.productId"
            class="product-card"
          >
            <div class="product-image-container" @click="viewProduct(product.slug)">
              <img
                :src="product.mainImageUrl || '/placeholder.png'"
                :alt="product.productName"
                class="product-image"
              />
            </div>
            <div class="product-info">
              <div class="product-name" @click="viewProduct(product.slug)">
                {{ product.productName }}
              </div>
              <div class="product-price">{{ formatPrice(product.price) }}</div>
              <div v-if="product.brand" class="product-brand">{{ product.brand }}</div>
              <button class="add-to-cart-btn" @click="addToCart(product)">
                Add to Cart
              </button>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="pagination">
          <button
            @click="page = page - 1"
            :disabled="page === 1"
          >
            Previous
          </button>
          <button
            v-for="p in totalPages"
            :key="p"
            :class="{ active: p === page }"
            @click="page = p"
          >
            {{ p }}
          </button>
          <button
            @click="page = page + 1"
            :disabled="page === totalPages"
          >
            Next
          </button>
        </div>
      </div>
    </div>

    <div v-else class="loading">
      Shop not found.
    </div>
  </div>
</template>

<style scoped>
.shop-page {
  min-height: 100vh;
}

.shop-header {
  height: 250px;
  background-color: #f5f5f5;
  background-size: cover;
  background-position: center;
  position: relative;
}

.shop-header-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  align-items: flex-end;
  padding: 2rem;
}

.shop-info {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}

.shop-logo {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #fff;
  background: #fff;
}

.shop-details {
  color: #fff;
}

.shop-name {
  font-size: 2rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.shop-description {
  font-size: 0.95rem;
  opacity: 0.9;
  margin-bottom: 0.5rem;
  max-width: 600px;
}

.shop-stats {
  display: flex;
  gap: 1.5rem;
  font-size: 0.9rem;
}

.shop-rating .stars {
  color: #ffc107;
  margin-right: 0.25rem;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1.5rem;
  padding: 2rem 0;
}

.product-card {
  background: #fff;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
  transition: box-shadow 0.3s;
}

.product-card:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12);
}

.product-image-container {
  position: relative;
  padding-top: 100%;
  background: var(--light-gray);
  cursor: pointer;
}

.product-image {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.product-info {
  padding: 1rem;
  text-align: center;
}

.product-name {
  font-size: 0.95rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
  color: var(--primary-color);
  cursor: pointer;
}

.product-name:hover {
  text-decoration: underline;
}

.product-price {
  font-size: 0.9rem;
  color: var(--secondary-color);
  margin-bottom: 0.5rem;
}

.product-brand {
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

.section-title {
  font-size: 1.5rem;
  font-weight: 400;
  margin-top: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-color);
}

.pagination {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  padding: 2rem;
}

.pagination button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color);
  background: #fff;
  cursor: pointer;
  border-radius: 4px;
}

.pagination button.active {
  background: var(--primary-color);
  color: #fff;
  border-color: var(--primary-color);
}

.pagination button:hover:not(.active):not(:disabled) {
  background: var(--light-gray);
}

.pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
