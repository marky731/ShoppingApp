import { defineStore } from 'pinia'
import { cartAPI } from '../services/api'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [],
    totalAmount: 0,
    loading: false,
    error: null
  }),

  getters: {
    totalItems: (state) => state.items.reduce((sum, item) => sum + item.quantity, 0),
    totalPrice: (state) => state.totalAmount || state.items.reduce((sum, item) => sum + (item.price * item.quantity), 0),
    cartItems: (state) => state.items
  },

  actions: {
    async fetchCart() {
      const token = localStorage.getItem('token')
      if (!token) {
        // Not logged in - use localStorage
        this.items = JSON.parse(localStorage.getItem('cart') || '[]')
        return
      }

      this.loading = true
      this.error = null
      try {
        const response = await cartAPI.get()
        this.items = response.data.items || []
        this.totalAmount = response.data.totalAmount || 0
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to load cart'
        // Fallback to localStorage on error
        this.items = JSON.parse(localStorage.getItem('cart') || '[]')
      } finally {
        this.loading = false
      }
    },

    async addItem(product, quantity = 1) {
      const token = localStorage.getItem('token')

      if (!token) {
        // Not logged in - use localStorage
        const existingItem = this.items.find(item => item.productId === product.productId)
        if (existingItem) {
          existingItem.quantity += quantity
        } else {
          this.items.push({
            productId: product.productId,
            productName: product.productName,
            price: product.price,
            mainImageUrl: product.mainImageUrl,
            quantity
          })
        }
        this.saveToLocalStorage()
        return true
      }

      this.loading = true
      this.error = null
      try {
        const response = await cartAPI.add(product.productId, quantity)
        this.items = response.data.items || []
        this.totalAmount = response.data.totalAmount || 0
        return true
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to add item'
        return false
      } finally {
        this.loading = false
      }
    },

    async removeItem(productId) {
      const token = localStorage.getItem('token')

      if (!token) {
        this.items = this.items.filter(item => item.productId !== productId)
        this.saveToLocalStorage()
        return true
      }

      this.loading = true
      try {
        const response = await cartAPI.remove(productId)
        this.items = response.data.items || []
        this.totalAmount = response.data.totalAmount || 0
        return true
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to remove item'
        return false
      } finally {
        this.loading = false
      }
    },

    async updateQuantity(productId, quantity) {
      if (quantity <= 0) {
        return this.removeItem(productId)
      }

      const token = localStorage.getItem('token')

      if (!token) {
        const item = this.items.find(item => item.productId === productId)
        if (item) {
          item.quantity = quantity
          this.saveToLocalStorage()
        }
        return true
      }

      this.loading = true
      try {
        const response = await cartAPI.update(productId, quantity)
        this.items = response.data.items || []
        this.totalAmount = response.data.totalAmount || 0
        return true
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to update quantity'
        return false
      } finally {
        this.loading = false
      }
    },

    async clearCart() {
      const token = localStorage.getItem('token')

      if (!token) {
        this.items = []
        this.saveToLocalStorage()
        return true
      }

      this.loading = true
      try {
        await cartAPI.clear()
        this.items = []
        this.totalAmount = 0
        return true
      } catch (error) {
        this.error = error.response?.data?.message || 'Failed to clear cart'
        return false
      } finally {
        this.loading = false
      }
    },

    // Sync localStorage cart to backend after login
    async syncCartAfterLogin() {
      const localCart = JSON.parse(localStorage.getItem('cart') || '[]')
      if (localCart.length > 0) {
        for (const item of localCart) {
          try {
            await cartAPI.add(item.productId, item.quantity)
          } catch (error) {
            console.error('Failed to sync item:', item.productId)
          }
        }
        localStorage.removeItem('cart')
      }
      await this.fetchCart()
    },

    saveToLocalStorage() {
      localStorage.setItem('cart', JSON.stringify(this.items))
    },

    // Clear cart on logout
    onLogout() {
      this.items = []
      this.totalAmount = 0
      localStorage.removeItem('cart')
    }
  }
})
