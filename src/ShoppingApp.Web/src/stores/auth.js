import { defineStore } from 'pinia'
import { authAPI } from '../services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('token') || null,
    loading: false,
    error: null
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    currentUser: (state) => state.user
  },

  actions: {
    async login(email, password) {
      this.loading = true
      this.error = null
      try {
        const response = await authAPI.login({ email, password })
        this.token = response.data.token
        localStorage.setItem('token', this.token)
        await this.fetchUser()
        return true
      } catch (error) {
        this.error = error.response?.data?.message || 'Login failed'
        return false
      } finally {
        this.loading = false
      }
    },

    async register(userData) {
      this.loading = true
      this.error = null
      try {
        await authAPI.register(userData)
        return true
      } catch (error) {
        this.error = error.response?.data?.message || 'Registration failed'
        return false
      } finally {
        this.loading = false
      }
    },

    async fetchUser() {
      if (!this.token) return
      try {
        const response = await authAPI.getMe()
        this.user = response.data
      } catch (error) {
        this.logout()
      }
    },

    logout() {
      this.user = null
      this.token = null
      localStorage.removeItem('token')
    }
  }
})
