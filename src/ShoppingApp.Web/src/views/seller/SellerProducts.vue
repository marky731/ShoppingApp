<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { sellerAPI, categoriesAPI } from '../../services/api'

const router = useRouter()

const products = ref([])
const categories = ref([])
const loading = ref(true)
const error = ref(null)
const page = ref(1)
const totalPages = ref(1)
const showForm = ref(false)
const editingProduct = ref(null)

const productForm = ref({
  productName: '',
  description: '',
  price: 0,
  stockQuantity: 0,
  categoryId: null,
  brand: '',
  mainImageUrl: ''
})

onMounted(async () => {
  await Promise.all([loadProducts(), loadCategories()])
})

const loadProducts = async () => {
  loading.value = true
  try {
    const response = await sellerAPI.getProducts({ page: page.value, pageSize: 10 })
    products.value = response.data.items || []
    totalPages.value = response.data.totalPages || 1
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load products'
  } finally {
    loading.value = false
  }
}

const loadCategories = async () => {
  try {
    const response = await categoriesAPI.getAll()
    // Flatten categories for select
    const flatCategories = []
    response.data.forEach(cat => {
      flatCategories.push(cat)
      if (cat.subCategories) {
        cat.subCategories.forEach(sub => {
          flatCategories.push({ ...sub, categoryName: `  └ ${sub.categoryName}` })
        })
      }
    })
    categories.value = flatCategories
  } catch (err) {
    console.error('Failed to load categories')
  }
}

const openAddForm = () => {
  editingProduct.value = null
  productForm.value = {
    productName: '',
    description: '',
    price: 0,
    stockQuantity: 0,
    categoryId: categories.value[0]?.categoryId || null,
    brand: '',
    mainImageUrl: ''
  }
  showForm.value = true
}

const openEditForm = async (product) => {
  editingProduct.value = product
  productForm.value = {
    productName: product.productName,
    description: product.description || '',
    price: product.price,
    stockQuantity: product.stockQuantity,
    categoryId: product.categoryId,
    brand: product.brand || '',
    mainImageUrl: product.mainImageUrl || ''
  }
  showForm.value = true
}

const saveProduct = async () => {
  if (!productForm.value.productName || !productForm.value.price) {
    error.value = 'Product name and price are required'
    return
  }

  loading.value = true
  error.value = null
  try {
    if (editingProduct.value) {
      await sellerAPI.updateProduct(editingProduct.value.productId, productForm.value)
    } else {
      await sellerAPI.createProduct(productForm.value)
    }
    showForm.value = false
    await loadProducts()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save product'
  } finally {
    loading.value = false
  }
}

const deleteProduct = async (productId) => {
  if (!confirm('Are you sure you want to delete this product?')) return

  try {
    await sellerAPI.deleteProduct(productId)
    await loadProducts()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to delete product'
  }
}

const formatPrice = (price) => `$${price.toFixed(2)}`
</script>

<template>
  <div class="seller-products">
    <div class="page-header">
      <h1>Products</h1>
      <button class="add-btn" @click="openAddForm">
        <i class="bi bi-plus-lg"></i> Add Product
      </button>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>

    <!-- Product Form Modal -->
    <div v-if="showForm" class="modal-overlay" @click.self="showForm = false">
      <div class="modal-content">
        <h2>{{ editingProduct ? 'Edit Product' : 'Add Product' }}</h2>

        <div class="form-grid">
          <div class="form-group">
            <label>Product Name *</label>
            <input v-model="productForm.productName" type="text" />
          </div>

          <div class="form-group">
            <label>Category</label>
            <select v-model="productForm.categoryId">
              <option v-for="cat in categories" :key="cat.categoryId" :value="cat.categoryId">
                {{ cat.categoryName }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Price *</label>
            <input v-model.number="productForm.price" type="number" min="0" step="0.01" />
          </div>

          <div class="form-group">
            <label>Stock Quantity</label>
            <input v-model.number="productForm.stockQuantity" type="number" min="0" />
          </div>

          <div class="form-group">
            <label>Brand</label>
            <input v-model="productForm.brand" type="text" />
          </div>

          <div class="form-group">
            <label>Image URL</label>
            <input v-model="productForm.mainImageUrl" type="url" />
          </div>

          <div class="form-group full-width">
            <label>Description</label>
            <textarea v-model="productForm.description" rows="3"></textarea>
          </div>
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="showForm = false">Cancel</button>
          <button class="save-btn" @click="saveProduct" :disabled="loading">
            {{ loading ? 'Saving...' : 'Save Product' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Products Table -->
    <div v-if="loading && !showForm" class="loading">Loading products...</div>

    <div v-else-if="products.length === 0" class="empty-state">
      <i class="bi bi-box-seam"></i>
      <p>No products yet. Add your first product!</p>
    </div>

    <table v-else class="products-table">
      <thead>
        <tr>
          <th>Image</th>
          <th>Name</th>
          <th>Price</th>
          <th>Stock</th>
          <th>Status</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="product in products"
          :key="product.productId"
          class="clickable-row"
          @click="openEditForm(product)"
        >
          <td>
            <img :src="product.mainImageUrl || '/placeholder.png'" :alt="product.productName" class="product-thumb" />
          </td>
          <td>{{ product.productName }}</td>
          <td>{{ formatPrice(product.price) }}</td>
          <td>{{ product.stockQuantity }}</td>
          <td>
            <span :class="['status-badge', product.isActive ? 'active' : 'inactive']">
              {{ product.isActive ? 'Active' : 'Inactive' }}
            </span>
          </td>
          <td class="action-cell" @click.stop>
            <button class="action-btn delete" @click="deleteProduct(product.productId)" title="Delete">
              <i class="bi bi-trash"></i>
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Pagination -->
    <div v-if="totalPages > 1" class="pagination">
      <button @click="page--; loadProducts()" :disabled="page === 1">Previous</button>
      <span>Page {{ page }} of {{ totalPages }}</span>
      <button @click="page++; loadProducts()" :disabled="page === totalPages">Next</button>
    </div>
  </div>
</template>

<style scoped>
.seller-products h1 {
  font-weight: 400;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.add-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--primary-color);
  color: #fff;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
}

.add-btn:hover {
  background: var(--secondary-color);
}

.products-table {
  width: 100%;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  border-collapse: collapse;
}

.products-table th, .products-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.products-table th {
  background: var(--light-gray);
  font-weight: 500;
}

.clickable-row {
  cursor: pointer;
  transition: background 0.2s;
}

.clickable-row:hover {
  background: var(--light-gray);
}

.action-cell {
  width: 50px;
  text-align: right;
}

.product-thumb {
  width: 50px;
  height: 50px;
  object-fit: cover;
  border-radius: 8px;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
}

.status-badge.active {
  background: #d4edda;
  color: #155724;
}

.status-badge.inactive {
  background: #f8d7da;
  color: #721c24;
}

.action-btn {
  background: none;
  border: 1px solid var(--border-color);
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  margin-right: 0.5rem;
}

.action-btn:hover {
  background: var(--light-gray);
}

.action-btn.delete {
  color: #dc3545;
}

.action-btn.delete:hover {
  background: #ffe6e6;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: #fff;
  padding: 2rem;
  border-radius: 12px;
  width: 90%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-content h2 {
  margin-bottom: 1.5rem;
  font-weight: 500;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group input, .form-group select, .form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.cancel-btn, .save-btn {
  flex: 1;
  padding: 0.75rem;
  border-radius: 8px;
  cursor: pointer;
}

.cancel-btn {
  background: #fff;
  border: 1px solid var(--border-color);
}

.save-btn {
  background: var(--primary-color);
  color: #fff;
  border: none;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: #fff;
  border-radius: 12px;
}

.empty-state i {
  font-size: 3rem;
  color: var(--secondary-color);
  margin-bottom: 1rem;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 1.5rem;
}

.pagination button {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color);
  background: #fff;
  border-radius: 6px;
  cursor: pointer;
}

.pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>
