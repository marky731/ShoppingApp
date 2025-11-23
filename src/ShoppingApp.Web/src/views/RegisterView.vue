<script setup>
import { ref } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const phone = ref('')

const handleRegister = async () => {
  const success = await authStore.register({
    firstName: firstName.value,
    lastName: lastName.value,
    email: email.value,
    password: password.value,
    confirmPassword: confirmPassword.value,
    phone: phone.value || null
  })
  if (success) {
    router.push('/login')
  }
}
</script>

<template>
  <div class="auth-container">
    <h1 class="auth-title">Register</h1>

    <div v-if="authStore.error" class="error-message">
      {{ authStore.error }}
    </div>

    <form @submit.prevent="handleRegister">
      <div class="form-group">
        <label for="firstName">First Name</label>
        <input
          id="firstName"
          v-model="firstName"
          type="text"
          required
        />
      </div>

      <div class="form-group">
        <label for="lastName">Last Name</label>
        <input
          id="lastName"
          v-model="lastName"
          type="text"
          required
        />
      </div>

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
          minlength="6"
        />
      </div>

      <div class="form-group">
        <label for="confirmPassword">Confirm Password</label>
        <input
          id="confirmPassword"
          v-model="confirmPassword"
          type="password"
          required
          minlength="6"
        />
      </div>

      <div class="form-group">
        <label for="phone">Phone (optional)</label>
        <input
          id="phone"
          v-model="phone"
          type="tel"
        />
      </div>

      <button type="submit" class="auth-btn" :disabled="authStore.loading">
        {{ authStore.loading ? 'Registering...' : 'Register' }}
      </button>
    </form>

    <p class="auth-link">
      Already have an account? <RouterLink to="/login">Login</RouterLink>
    </p>
  </div>
</template>
