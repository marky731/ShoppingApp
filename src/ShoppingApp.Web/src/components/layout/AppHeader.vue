<script setup>
import { ref, onMounted, computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useCartStore } from '../../stores/cart'
import { categoriesAPI } from '../../services/api'

const router = useRouter()
const authStore = useAuthStore()
const cartStore = useCartStore()

const categories = ref([])
const searchQuery = ref('')

const isLoggedIn = computed(() => authStore.isAuthenticated)
const cartCount = computed(() => cartStore.totalItems)

onMounted(async () => {
  try {
    const response = await categoriesAPI.getAll()
    categories.value = response.data.filter(c => !c.parentCategoryId)
  } catch (error) {
    console.error('Failed to load categories:', error)
  }
})

const handleSearch = () => {
  if (searchQuery.value.trim()) {
    router.push({ name: 'products', query: { search: searchQuery.value } })
  }
}

const handleLogout = () => {
  authStore.logout()
  router.push('/')
}
</script>

<template>
  <header class="app-header">
    <div class="header-top">
      <RouterLink to="/" class="logo">
        <span class="logo-bold">MINIMAL</span><span class="logo-light">SHOP</span>
      </RouterLink>

      <div class="header-icons">
        <RouterLink to="/favorites" class="header-icon" title="Favorites">
          ♡
        </RouterLink>
        <RouterLink to="/cart" class="header-icon" title="Cart">
          🛒
          <span v-if="cartCount > 0" class="badge-count">{{ cartCount }}</span>
        </RouterLink>
        <template v-if="isLoggedIn">
          <button class="header-icon" title="Account" @click="handleLogout">
            👤
          </button>
        </template>
        <template v-else>
          <RouterLink to="/login" class="header-icon" title="Login">
            👤
          </RouterLink>
        </template>
        <button class="header-icon" title="Notifications">
          🔔
        </button>
      </div>
    </div>

    <nav class="nav-categories">
      <ul class="nav-links">
        <li>
          <RouterLink to="/products">All Categories</RouterLink>
        </li>
        <li v-for="category in categories" :key="category.categoryId">
          <RouterLink :to="{ name: 'category', params: { id: category.categoryId } }">
            {{ category.categoryName }}
          </RouterLink>
        </li>
      </ul>

      <div class="search-box">
        <span class="search-icon">🔍</span>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search for products..."
          @keyup.enter="handleSearch"
        />
      </div>
    </nav>
  </header>
</template>
