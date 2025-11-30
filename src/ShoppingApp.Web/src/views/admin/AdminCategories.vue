<script setup>
import { ref, onMounted } from 'vue'
import { adminAPI, categoriesAPI } from '../../services/api'

const categories = ref([])
const loading = ref(true)
const error = ref(null)
const showForm = ref(false)
const editingCategory = ref(null)

const categoryForm = ref({
  categoryName: '',
  parentCategoryId: null
})

onMounted(async () => {
  await loadCategories()
})

const loadCategories = async () => {
  loading.value = true
  try {
    const response = await categoriesAPI.getAll()
    categories.value = response.data || []
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load categories'
  } finally {
    loading.value = false
  }
}

const openAddForm = (parentId = null) => {
  editingCategory.value = null
  categoryForm.value = {
    categoryName: '',
    parentCategoryId: parentId
  }
  showForm.value = true
}

const openEditForm = (category, parentId = null) => {
  editingCategory.value = category
  categoryForm.value = {
    categoryName: category.categoryName,
    parentCategoryId: parentId
  }
  showForm.value = true
}

const saveCategory = async () => {
  if (!categoryForm.value.categoryName.trim()) {
    error.value = 'Category name is required'
    return
  }

  loading.value = true
  error.value = null
  try {
    if (editingCategory.value) {
      await adminAPI.updateCategory(editingCategory.value.categoryId, categoryForm.value)
    } else {
      await adminAPI.createCategory(categoryForm.value)
    }
    showForm.value = false
    await loadCategories()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save category'
  } finally {
    loading.value = false
  }
}

const deleteCategory = async (categoryId) => {
  if (!confirm('Are you sure you want to delete this category?')) return
  try {
    await adminAPI.deleteCategory(categoryId)
    await loadCategories()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to delete category'
  }
}

const getParentOptions = () => {
  // Flatten categories for select (only top-level as parents)
  return categories.value.map(cat => ({
    categoryId: cat.categoryId,
    categoryName: cat.categoryName
  }))
}
</script>

<template>
  <div class="admin-categories">
    <div class="page-header">
      <h1>Category Management</h1>
      <button class="add-btn" @click="openAddForm()">
        <i class="bi bi-plus-lg"></i> Add Category
      </button>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>

    <!-- Category Form Modal -->
    <div v-if="showForm" class="modal-overlay" @click.self="showForm = false">
      <div class="modal-content">
        <h2>{{ editingCategory ? 'Edit Category' : 'Add Category' }}</h2>

        <div class="form-group">
          <label>Category Name *</label>
          <input v-model="categoryForm.categoryName" type="text" placeholder="e.g. Electronics" />
        </div>

        <div class="form-group">
          <label>Parent Category</label>
          <select v-model="categoryForm.parentCategoryId">
            <option :value="null">None (Top Level)</option>
            <option
              v-for="cat in getParentOptions()"
              :key="cat.categoryId"
              :value="cat.categoryId"
              :disabled="editingCategory && cat.categoryId === editingCategory.categoryId"
            >
              {{ cat.categoryName }}
            </option>
          </select>
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="showForm = false">Cancel</button>
          <button class="save-btn" @click="saveCategory" :disabled="loading">
            {{ loading ? 'Saving...' : 'Save Category' }}
          </button>
        </div>
      </div>
    </div>

    <div v-if="loading && !showForm" class="loading">Loading categories...</div>

    <div v-else-if="categories.length === 0" class="empty-state">
      <i class="bi bi-grid"></i>
      <p>No categories yet. Create your first category!</p>
    </div>

    <div v-else class="categories-list">
      <div v-for="category in categories" :key="category.categoryId" class="category-item">
        <div class="category-main clickable-row" @click="openEditForm(category)">
          <div class="category-info">
            <span class="category-name">{{ category.categoryName }}</span>
            <span class="sub-count" v-if="category.subCategories?.length">
              ({{ category.subCategories.length }} subcategories)
            </span>
          </div>
          <div class="category-actions" @click.stop>
            <button class="action-btn" @click="openAddForm(category.categoryId)" title="Add Subcategory">
              <i class="bi bi-plus"></i>
            </button>
            <button class="action-btn delete" @click="deleteCategory(category.categoryId)" title="Delete">
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </div>

        <div v-if="category.subCategories?.length" class="subcategories">
          <div
            v-for="sub in category.subCategories"
            :key="sub.categoryId"
            class="subcategory-item clickable-row"
            @click="openEditForm(sub, category.categoryId)"
          >
            <div class="category-info">
              <i class="bi bi-arrow-return-right"></i>
              <span class="category-name">{{ sub.categoryName }}</span>
            </div>
            <div class="category-actions" @click.stop>
              <button class="action-btn delete" @click="deleteCategory(sub.categoryId)" title="Delete">
                <i class="bi bi-trash"></i>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-categories h1 {
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
  background: #1a1a2e;
  color: #fff;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  cursor: pointer;
}

.add-btn:hover {
  background: #16213e;
}

.categories-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.category-item {
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.category-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
}

.clickable-row {
  cursor: pointer;
  transition: background 0.2s;
}

.clickable-row:hover {
  background: var(--light-gray);
}

.category-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.category-name {
  font-weight: 500;
}

.sub-count {
  font-size: 0.85rem;
  color: var(--secondary-color);
}

.category-actions {
  display: flex;
  gap: 0.25rem;
}

.action-btn {
  background: none;
  border: 1px solid var(--border-color);
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
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

.subcategories {
  border-top: 1px solid var(--border-color);
  background: var(--light-gray);
}

.subcategory-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1.5rem 0.75rem 2.5rem;
  border-bottom: 1px solid var(--border-color);
}

.subcategory-item:last-child {
  border-bottom: none;
}

.subcategory-item .category-info i {
  color: var(--secondary-color);
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
  max-width: 400px;
}

.modal-content h2 {
  margin-bottom: 1.5rem;
  font-weight: 500;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group input, .form-group select {
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
  background: #1a1a2e;
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
</style>
