<script setup>
import { ref, onMounted } from 'vue'
import { productsAPI } from '../services/api'
import HeroSection from '../components/shared/HeroSection.vue'
import ProductCard from '../components/shared/ProductCard.vue'

const products = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    const response = await productsAPI.getAll({ pageSize: 8 })
    products.value = response.data.items
  } catch (error) {
    console.error('Failed to load products:', error)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <HeroSection />

    <section class="page-container">
      <h2 class="section-title">Featured Products</h2>

      <div v-if="loading" class="loading">
        Loading products...
      </div>

      <div v-else class="products-grid">
        <ProductCard
          v-for="product in products"
          :key="product.productId"
          :product="product"
        />
      </div>
    </section>
  </div>
</template>
