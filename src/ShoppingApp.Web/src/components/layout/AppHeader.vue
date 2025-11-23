<script setup>
import { ref, onMounted, computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useCartStore } from '../../stores/cart'
import { categoriesAPI } from '../../services/api'

const router = useRouter()
const authStore = useAuthStore()
const cartStore = useCartStore()

const allCategories = ref([])
const searchQuery = ref('')

const isLoggedIn = computed(() => authStore.isAuthenticated)
const cartCount = computed(() => cartStore.totalItems)

// Get parent categories with their subcategories (API returns nested data)
const categoriesWithSubs = computed(() => {
  const parents = allCategories.value.filter(c => !c.parentCategoryId)
  return parents.slice(0, 7).map(parent => ({
    ...parent,
    subcategories: parent.subCategories || []
  }))
})

onMounted(async () => {
  try {
    const response = await categoriesAPI.getAll()
    allCategories.value = response.data
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
          <i class="bi bi-heart"></i>
        </RouterLink>
        <RouterLink to="/cart" class="header-icon" title="Cart">
          <i class="bi bi-cart3"></i>
          <span v-if="cartCount > 0" class="badge-count">{{ cartCount }}</span>
        </RouterLink>
        <template v-if="isLoggedIn">
          <button class="header-icon" title="Logout" @click="handleLogout">
            <i class="bi bi-person-fill"></i>
          </button>
        </template>
        <template v-else>
          <RouterLink to="/login" class="header-icon" title="Login">
            <i class="bi bi-person"></i>
          </RouterLink>
        </template>
        <button class="header-icon" title="Notifications">
          <i class="bi bi-bell"></i>
        </button>
      </div>
    </div>

    <nav class="nav-categories">
      <ul class="nav-links">
        <li>
          <RouterLink to="/products">All Categories</RouterLink>
        </li>
        <li
          v-for="category in categoriesWithSubs"
          :key="category.categoryId"
          class="nav-category-dropdown"
        >
          <RouterLink :to="{ name: 'category', params: { id: category.categoryId } }">
            {{ category.categoryName }}
          </RouterLink>
          <ul v-if="category.subcategories.length > 0" class="subcategory-menu">
            <li v-for="sub in category.subcategories" :key="sub.categoryId">
              <RouterLink :to="{ name: 'category', params: { id: sub.categoryId } }">
                {{ sub.categoryName }}
              </RouterLink>
            </li>
          </ul>
        </li>
      </ul>

      <div class="search-box">
        <i class="bi bi-search search-icon"></i>
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
