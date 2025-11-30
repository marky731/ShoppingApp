<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const authStore = useAuthStore()

const isLoggedIn = computed(() => authStore.isAuthenticated)
const userRole = computed(() => authStore.currentUser?.role)
const userName = computed(() => authStore.currentUser?.firstName || 'User')
</script>

<template>
  <section class="hero-section">
    <template v-if="!isLoggedIn">
      <h1 class="hero-title">Discover Your Style</h1>
      <p class="hero-subtitle">Minimal designs for a modern life.</p>
      <RouterLink to="/products" class="hero-btn">
        Shop Now
      </RouterLink>
    </template>
    <template v-else>
      <h1 class="hero-title">Welcome back, {{ userName }}!</h1>
      <p class="hero-subtitle" v-if="userRole === 'admin'">
        Manage your platform from the admin dashboard.
      </p>
      <p class="hero-subtitle" v-else-if="userRole === 'seller'">
        Manage your shop and products from the seller dashboard.
      </p>
      <p class="hero-subtitle" v-else>
        Continue exploring our minimal designs.
      </p>
      <RouterLink v-if="userRole === 'admin'" to="/admin" class="hero-btn">
        Go to Admin Dashboard
      </RouterLink>
      <RouterLink v-else-if="userRole === 'seller'" to="/seller" class="hero-btn">
        Go to Seller Dashboard
      </RouterLink>
      <RouterLink v-else to="/products" class="hero-btn">
        Browse Products
      </RouterLink>
    </template>
  </section>
</template>
