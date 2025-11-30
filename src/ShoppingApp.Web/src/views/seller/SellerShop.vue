<script setup>
import { ref, onMounted, defineProps, defineEmits } from 'vue'
import { sellerAPI } from '../../services/api'

const props = defineProps(['shop', 'hasShop'])
const emit = defineEmits(['shop-updated'])

const formData = ref({
  shopName: '',
  description: '',
  logoImageUrl: '',
  bannerImageUrl: ''
})
const loading = ref(false)
const error = ref(null)
const success = ref(null)

onMounted(() => {
  if (props.shop) {
    formData.value = {
      shopName: props.shop.shopName || '',
      description: props.shop.description || '',
      logoImageUrl: props.shop.logoImageUrl || '',
      bannerImageUrl: props.shop.bannerImageUrl || ''
    }
  }
})

const saveShop = async () => {
  if (!formData.value.shopName.trim()) {
    error.value = 'Shop name is required'
    return
  }

  loading.value = true
  error.value = null
  success.value = null

  try {
    if (props.hasShop) {
      await sellerAPI.updateShop(formData.value)
      success.value = 'Shop updated successfully'
    } else {
      await sellerAPI.createShop(formData.value)
      success.value = 'Shop created successfully! Waiting for admin approval.'
    }
    emit('shop-updated')
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to save shop'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="seller-shop">
    <h1>{{ hasShop ? 'Edit Shop Profile' : 'Create Your Shop' }}</h1>

    <div v-if="error" class="error-message">{{ error }}</div>
    <div v-if="success" class="success-message">{{ success }}</div>

    <div v-if="shop && !shop.isApproved" class="pending-badge">
      <i class="bi bi-hourglass-split"></i>
      Pending Approval
    </div>

    <div class="form-container">
      <div class="form-group">
        <label>Shop Name *</label>
        <input v-model="formData.shopName" type="text" placeholder="Enter your shop name" />
      </div>

      <div class="form-group">
        <label>Description</label>
        <textarea v-model="formData.description" rows="4" placeholder="Tell customers about your shop"></textarea>
      </div>

      <div class="form-group">
        <label>Logo Image URL</label>
        <input v-model="formData.logoImageUrl" type="url" placeholder="https://..." />
        <div v-if="formData.logoImageUrl" class="image-preview">
          <img :src="formData.logoImageUrl" alt="Logo preview" />
        </div>
      </div>

      <div class="form-group">
        <label>Banner Image URL</label>
        <input v-model="formData.bannerImageUrl" type="url" placeholder="https://..." />
        <div v-if="formData.bannerImageUrl" class="image-preview banner">
          <img :src="formData.bannerImageUrl" alt="Banner preview" />
        </div>
      </div>

      <button class="save-btn" @click="saveShop" :disabled="loading">
        {{ loading ? 'Saving...' : (hasShop ? 'Update Shop' : 'Create Shop') }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.seller-shop {
  max-width: 600px;
}

.seller-shop h1 {
  font-weight: 400;
  margin-bottom: 1.5rem;
}

.pending-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  background: #fff3cd;
  color: #856404;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
}

.form-container {
  background: #fff;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.form-group input, .form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  font-size: 1rem;
}

.form-group input:focus, .form-group textarea:focus {
  outline: none;
  border-color: var(--primary-color);
}

.image-preview {
  margin-top: 1rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  overflow: hidden;
}

.image-preview img {
  width: 100px;
  height: 100px;
  object-fit: cover;
}

.image-preview.banner img {
  width: 100%;
  height: 150px;
}

.save-btn {
  width: 100%;
  background: var(--primary-color);
  color: #fff;
  border: none;
  padding: 1rem;
  border-radius: 8px;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.2s;
}

.save-btn:hover:not(:disabled) {
  background: var(--secondary-color);
}

.save-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.success-message {
  background: #d4edda;
  color: #155724;
  padding: 0.75rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}

.error-message {
  background: #f8d7da;
  color: #721c24;
  padding: 0.75rem;
  border-radius: 8px;
  margin-bottom: 1rem;
}
</style>
