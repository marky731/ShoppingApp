<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { productsAPI } from '../services/api'
import ProductCard from '../components/shared/ProductCard.vue'

const route = useRoute()

const products = ref([])
const loading = ref(true)
const currentPage = ref(1)
const totalPages = ref(1)
const sortBy = ref('')

const loadProducts = async () => {
  loading.value = true
  try {
    const params = {
      page: currentPage.value,
      pageSize: 12,
      sortBy: sortBy.value || undefined,
      search: route.query.search || undefined,
      categoryId: route.params.id || undefined
    }

    const response = await productsAPI.getAll(params)
    products.value = response.data.items
    totalPages.value = response.data.totalPages
  } catch (error) {
    console.error('Failed to load products:', error)
  } finally {
    loading.value = false
  }
}

onMounted(loadProducts)

watch(() => route.query, loadProducts)
watch(() => route.params.id, loadProducts)
watch(sortBy, () => {
  currentPage.value = 1
  loadProducts()
})

const changePage = (page) => {
  currentPage.value = page
  loadProducts()
}
</script>

<template>
  <div>
    <div class="filters-container">
      <select v-model="sortBy" class="filter-select">
        <option value="">Sort by</option>
        <option value="price_asc">Price: Low to High</option>
        <option value="price_desc">Price: High to Low</option>
        <option value="newest">Newest</option>
        <option value="rating">Rating</option>
      </select>
    </div>

    <div v-if="loading" class="loading">
      Loading products...
    </div>

    <div v-else>
      <div class="products-grid">
        <ProductCard
          v-for="product in products"
          :key="product.productId"
          :product="product"
        />
      </div>

      <div v-if="products.length === 0" class="loading">
        No products found.
      </div>

      <div v-if="totalPages > 1" class="pagination">
        <button
          v-for="page in totalPages"
          :key="page"
          :class="{ active: page === currentPage }"
          @click="changePage(page)"
        >
          {{ page }}
        </button>
      </div>
    </div>
  </div>
</template>
