import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5001/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// Auth APIs
export const authAPI = {
  register: (data) => api.post('/auth/register', data),
  login: (data) => api.post('/auth/login', data),
  getMe: () => api.get('/auth/me')
}

// Products APIs
export const productsAPI = {
  getAll: (params) => api.get('/products', { params }),
  getById: (id) => api.get(`/products/${id}`),
  getBySlug: (slug) => api.get(`/products/slug/${slug}`)
}

// Categories APIs
export const categoriesAPI = {
  getAll: () => api.get('/categories'),
  getById: (id) => api.get(`/categories/${id}`)
}

// Cart APIs
export const cartAPI = {
  get: () => api.get('/cart'),
  add: (productId, quantity = 1) => api.post('/cart', { productId, quantity }),
  update: (productId, quantity) => api.put(`/cart/${productId}`, { quantity }),
  remove: (productId) => api.delete(`/cart/${productId}`),
  clear: () => api.delete('/cart')
}

// Favorites APIs
export const favoritesAPI = {
  get: () => api.get('/favorites'),
  add: (productId) => api.post(`/favorites/${productId}`),
  remove: (productId) => api.delete(`/favorites/${productId}`),
  check: (productId) => api.get(`/favorites/check/${productId}`)
}

// Address APIs
export const addressAPI = {
  getAll: () => api.get('/addresses'),
  getById: (id) => api.get(`/addresses/${id}`),
  create: (data) => api.post('/addresses', data),
  update: (id, data) => api.put(`/addresses/${id}`, data),
  delete: (id) => api.delete(`/addresses/${id}`)
}

// Orders APIs
export const ordersAPI = {
  getAll: () => api.get('/orders'),
  getById: (id) => api.get(`/orders/${id}`),
  create: (shippingAddressId, discountCode = null) =>
    api.post('/orders', { shippingAddressId, discountCode }),
  cancel: (id) => api.post(`/orders/${id}/cancel`)
}

// Shops APIs
export const shopsAPI = {
  getAll: () => api.get('/shops'),
  getById: (id, params) => api.get(`/shops/${id}`, { params })
}

// User Profile APIs
export const profileAPI = {
  get: () => api.get('/auth/me'),
  update: (data) => api.put('/auth/profile', data)
}

// Reviews APIs
export const reviewsAPI = {
  getProductReviews: (productId) => api.get(`/reviews/product/${productId}`),
  canReview: (productId) => api.get(`/reviews/can-review/${productId}`),
  create: (data) => api.post('/reviews', data),
  update: (id, data) => api.put(`/reviews/${id}`, data),
  delete: (id) => api.delete(`/reviews/${id}`),
  getMyReviews: () => api.get('/reviews/my-reviews')
}

// Seller APIs
export const sellerAPI = {
  // Shop
  getShop: () => api.get('/seller/shop'),
  createShop: (data) => api.post('/seller/shop', data),
  updateShop: (data) => api.put('/seller/shop', data),
  getStats: () => api.get('/seller/stats'),

  // Products
  getProducts: (params) => api.get('/seller/products', { params }),
  getProduct: (id) => api.get(`/seller/products/${id}`),
  createProduct: (data) => api.post('/seller/products', data),
  updateProduct: (id, data) => api.put(`/seller/products/${id}`, data),
  deleteProduct: (id) => api.delete(`/seller/products/${id}`),

  // Orders
  getOrders: (params) => api.get('/seller/orders', { params }),
  getOrder: (id) => api.get(`/seller/orders/${id}`),
  updateOrderStatus: (id, status) => api.put(`/seller/orders/${id}/status`, { status }),
  updateOrderTracking: (id, trackingNumber) => api.put(`/seller/orders/${id}/tracking`, { trackingNumber }),

  // Discounts
  getDiscounts: () => api.get('/seller/discounts'),
  createDiscount: (data) => api.post('/seller/discounts', data),
  updateDiscount: (id, data) => api.put(`/seller/discounts/${id}`, data),
  deleteDiscount: (id) => api.delete(`/seller/discounts/${id}`)
}

// Admin APIs
export const adminAPI = {
  // Stats
  getStats: () => api.get('/admin/stats'),

  // Users
  getUsers: (params) => api.get('/admin/users', { params }),
  suspendUser: (id) => api.put(`/admin/users/${id}/suspend`),
  activateUser: (id) => api.put(`/admin/users/${id}/activate`),
  deleteUser: (id) => api.delete(`/admin/users/${id}`),

  // Sellers
  getPendingSellers: () => api.get('/admin/sellers/pending'),
  approveSeller: (id) => api.put(`/admin/sellers/${id}/approve`),
  rejectSeller: (id) => api.put(`/admin/sellers/${id}/reject`),

  // Categories
  createCategory: (data) => api.post('/admin/categories', data),
  updateCategory: (id, data) => api.put(`/admin/categories/${id}`, data),
  deleteCategory: (id) => api.delete(`/admin/categories/${id}`),

  // Reviews
  getPendingReviews: () => api.get('/admin/reviews/pending'),
  approveReview: (id) => api.put(`/admin/reviews/${id}/approve`),
  rejectReview: (id) => api.put(`/admin/reviews/${id}/reject`)
}

export default api
