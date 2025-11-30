<script setup>
import { computed } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const props = defineProps({
  isOpen: Boolean
})

const emit = defineEmits(['close'])

const router = useRouter()
const authStore = useAuthStore()

const user = computed(() => authStore.currentUser)
const isSeller = computed(() => user.value?.role === 'seller')
const isAdmin = computed(() => user.value?.role === 'admin')

const handleLogout = () => {
  authStore.logout()
  emit('close')
  router.push('/')
}

const navigateTo = (path) => {
  emit('close')
  router.push(path)
}
</script>

<template>
  <Teleport to="body">
    <!-- Overlay -->
    <div
      v-if="isOpen"
      class="sidebar-overlay"
      @click="emit('close')"
    ></div>

    <!-- Sidebar -->
    <aside class="profile-sidebar" :class="{ open: isOpen }">
      <div class="sidebar-header">
        <h2>My Profile</h2>
        <button class="close-btn" @click="emit('close')">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>

      <div v-if="user" class="user-info">
        <div class="user-avatar">
          <i class="bi bi-person-circle"></i>
        </div>
        <div class="user-details">
          <div class="user-name">{{ user.firstName }} {{ user.lastName }}</div>
          <div class="user-email">{{ user.email }}</div>
          <span class="user-role" :class="user.role">{{ user.role }}</span>
        </div>
      </div>

      <nav class="sidebar-nav">
        <button class="nav-item" @click="navigateTo('/orders')">
          <i class="bi bi-bag"></i>
          <span>My Orders</span>
        </button>
        <button class="nav-item" @click="navigateTo('/favorites')">
          <i class="bi bi-heart"></i>
          <span>My Favorites</span>
        </button>
        <button class="nav-item" @click="navigateTo('/cart')">
          <i class="bi bi-cart3"></i>
          <span>Shopping Cart</span>
        </button>
        <button class="nav-item" @click="navigateTo('/profile')">
          <i class="bi bi-geo-alt"></i>
          <span>My Addresses</span>
        </button>

        <div class="nav-divider"></div>

        <button v-if="isSeller" class="nav-item seller" @click="navigateTo('/seller')">
          <i class="bi bi-shop"></i>
          <span>Seller Dashboard</span>
        </button>
        <button v-if="isAdmin" class="nav-item admin" @click="navigateTo('/admin')">
          <i class="bi bi-shield-check"></i>
          <span>Admin Dashboard</span>
        </button>
      </nav>

      <div class="sidebar-footer">
        <button class="logout-btn" @click="handleLogout">
          <i class="bi bi-box-arrow-right"></i>
          <span>Logout</span>
        </button>
      </div>
    </aside>
  </Teleport>
</template>

<style scoped>
.sidebar-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  z-index: 999;
}

.profile-sidebar {
  position: fixed;
  top: 0;
  right: -320px;
  width: 320px;
  height: 100vh;
  background: #fff;
  box-shadow: -4px 0 20px rgba(0, 0, 0, 0.1);
  z-index: 1000;
  display: flex;
  flex-direction: column;
  transition: right 0.3s ease;
}

.profile-sidebar.open {
  right: 0;
}

.sidebar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--border-color);
}

.sidebar-header h2 {
  font-size: 1.25rem;
  font-weight: 500;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  color: var(--secondary-color);
  padding: 0.25rem;
}

.close-btn:hover {
  color: var(--primary-color);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem;
  background: var(--light-gray);
}

.user-avatar {
  font-size: 3rem;
  color: var(--secondary-color);
}

.user-details {
  flex: 1;
}

.user-name {
  font-weight: 600;
  font-size: 1.1rem;
  margin-bottom: 0.25rem;
}

.user-email {
  font-size: 0.85rem;
  color: var(--secondary-color);
  margin-bottom: 0.5rem;
}

.user-role {
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 12px;
  font-size: 0.7rem;
  text-transform: uppercase;
  font-weight: 600;
  letter-spacing: 0.5px;
  background: var(--border-color);
  color: var(--secondary-color);
}

.user-role.admin {
  background: #dc3545;
  color: #fff;
}

.user-role.seller {
  background: #6f42c1;
  color: #fff;
}

.sidebar-nav {
  flex: 1;
  padding: 1rem 0;
  overflow-y: auto;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  padding: 0.875rem 1.5rem;
  background: none;
  border: none;
  font-size: 0.95rem;
  color: var(--primary-color);
  cursor: pointer;
  transition: background 0.2s;
  text-align: left;
}

.nav-item:hover {
  background: var(--light-gray);
}

.nav-item i {
  font-size: 1.1rem;
  width: 24px;
}

.nav-item.seller {
  color: #6f42c1;
}

.nav-item.admin {
  color: #dc3545;
}

.nav-divider {
  height: 1px;
  background: var(--border-color);
  margin: 0.75rem 1.5rem;
}

.sidebar-footer {
  padding: 1rem 1.5rem;
  border-top: 1px solid var(--border-color);
}

.logout-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  padding: 0.875rem;
  background: #dc3545;
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 0.95rem;
  cursor: pointer;
  transition: background 0.2s;
}

.logout-btn:hover {
  background: #c82333;
}
</style>
