<script setup>
import { ref } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')

const handleLogin = async () => {
  const success = await authStore.login(email.value, password.value)
  if (success) {
    // Redirect based on user role
    const role = authStore.currentUser?.role
    if (role === 'admin') {
      router.push('/admin')
    } else if (role === 'seller') {
      router.push('/seller')
    } else {
      router.push('/')
    }
  }
}
</script>

<template>
  <div class="auth-container">
    <h1 class="auth-title">Login</h1>

    <div v-if="authStore.error" class="error-message">
      {{ authStore.error }}
    </div>

    <form @submit.prevent="handleLogin">
      <div class="form-group">
        <label for="email">Email</label>
        <input
          id="email"
          v-model="email"
          type="email"
          required
        />
      </div>

      <div class="form-group">
        <label for="password">Password</label>
        <input
          id="password"
          v-model="password"
          type="password"
          required
        />
      </div>

      <button type="submit" class="auth-btn" :disabled="authStore.loading">
        {{ authStore.loading ? 'Logging in...' : 'Login' }}
      </button>
    </form>

    <p class="auth-link">
      Don't have an account? <RouterLink to="/register">Register</RouterLink>
    </p>
  </div>
</template>
