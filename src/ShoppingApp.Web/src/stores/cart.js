import { defineStore } from 'pinia'

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: JSON.parse(localStorage.getItem('cart') || '[]'),
    loading: false
  }),

  getters: {
    totalItems: (state) => state.items.reduce((sum, item) => sum + item.quantity, 0),
    totalPrice: (state) => state.items.reduce((sum, item) => sum + (item.price * item.quantity), 0),
    cartItems: (state) => state.items
  },

  actions: {
    addItem(product, quantity = 1) {
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
    },

    removeItem(productId) {
      this.items = this.items.filter(item => item.productId !== productId)
      this.saveToLocalStorage()
    },

    updateQuantity(productId, quantity) {
      const item = this.items.find(item => item.productId === productId)
      if (item) {
        item.quantity = quantity
        if (item.quantity <= 0) {
          this.removeItem(productId)
        } else {
          this.saveToLocalStorage()
        }
      }
    },

    clearCart() {
      this.items = []
      this.saveToLocalStorage()
    },

    saveToLocalStorage() {
      localStorage.setItem('cart', JSON.stringify(this.items))
    }
  }
})
