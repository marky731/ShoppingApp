<script setup>
import { onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from './stores/auth'
import AppHeader from './components/layout/AppHeader.vue'
import AppFooter from './components/layout/AppFooter.vue'

const authStore = useAuthStore()
const route = useRoute()

// Hide header/footer on dashboard routes
const isDashboardRoute = computed(() => {
  return route.path.startsWith('/admin') || route.path.startsWith('/seller')
})

onMounted(() => {
  if (authStore.token) {
    authStore.fetchUser()
  }
})
</script>

<template>
  <div id="app">
    <AppHeader v-if="!isDashboardRoute" />
    <main :class="{ 'dashboard-main': isDashboardRoute }">
      <router-view />
    </main>
    <AppFooter v-if="!isDashboardRoute" />
  </div>
</template>

<style>
#app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

main {
  flex: 1;
}

main.dashboard-main {
  padding: 0;
  background: var(--light-gray, #f5f5f5);
}
</style>
