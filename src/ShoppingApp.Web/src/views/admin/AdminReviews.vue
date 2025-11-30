<script setup>
import { ref, onMounted, defineEmits } from 'vue'
import { adminAPI } from '../../services/api'

const emit = defineEmits(['refresh'])

const reviews = ref([])
const loading = ref(true)
const error = ref(null)

onMounted(async () => {
  await loadReviews()
})

const loadReviews = async () => {
  loading.value = true
  try {
    const response = await adminAPI.getPendingReviews()
    reviews.value = response.data || []
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load pending reviews'
  } finally {
    loading.value = false
  }
}

const approveReview = async (reviewId) => {
  try {
    await adminAPI.approveReview(reviewId)
    await loadReviews()
    emit('refresh')
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to approve review'
  }
}

const rejectReview = async (reviewId) => {
  if (!confirm('Are you sure you want to reject this review?')) return
  try {
    await adminAPI.rejectReview(reviewId)
    await loadReviews()
    emit('refresh')
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to reject review'
  }
}

const formatDate = (date) => new Date(date).toLocaleDateString('en-US', {
  year: 'numeric', month: 'short', day: 'numeric'
})

const renderStars = (rating) => {
  return Array(5).fill(0).map((_, i) => i < rating ? 'bi-star-fill' : 'bi-star')
}
</script>

<template>
  <div class="admin-reviews">
    <h1>Review Moderation</h1>

    <div v-if="error" class="error-message">{{ error }}</div>

    <div v-if="loading" class="loading">Loading pending reviews...</div>

    <div v-else-if="reviews.length === 0" class="empty-state">
      <i class="bi bi-check-circle"></i>
      <p>No pending reviews to moderate.</p>
    </div>

    <div v-else class="reviews-list">
      <div v-for="review in reviews" :key="review.reviewId" class="review-card">
        <div class="review-header">
          <div class="rating-stars">
            <i v-for="(star, i) in renderStars(review.rating)" :key="i" :class="['bi', star]"></i>
          </div>
          <span class="review-date">{{ formatDate(review.createdAt) }}</span>
        </div>

        <div class="product-info">
          <span class="product-label">Product:</span>
          <span class="product-name">{{ review.productName }}</span>
        </div>

        <div v-if="review.title" class="review-title">{{ review.title }}</div>
        <p v-if="review.comment" class="review-comment">{{ review.comment }}</p>
        <p v-else class="no-comment">No comment provided</p>

        <div class="reviewer-info">
          <span class="reviewer-label">Reviewer:</span>
          <span>{{ review.reviewer.firstName }} {{ review.reviewer.lastName }} ({{ review.reviewer.email }})</span>
        </div>

        <div class="card-actions">
          <button class="reject-btn" @click="rejectReview(review.reviewId)">
            <i class="bi bi-x-lg"></i> Reject
          </button>
          <button class="approve-btn" @click="approveReview(review.reviewId)">
            <i class="bi bi-check-lg"></i> Approve
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-reviews h1 {
  font-weight: 400;
  margin-bottom: 1.5rem;
}

.reviews-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.review-card {
  background: #fff;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.rating-stars {
  color: #ffc107;
  font-size: 1.1rem;
}

.review-date {
  font-size: 0.85rem;
  color: var(--secondary-color);
}

.product-info {
  margin-bottom: 1rem;
  padding: 0.75rem;
  background: var(--light-gray);
  border-radius: 8px;
}

.product-label, .reviewer-label {
  font-weight: 500;
  color: var(--secondary-color);
  margin-right: 0.5rem;
}

.product-name {
  font-weight: 500;
}

.review-title {
  font-weight: 600;
  margin-bottom: 0.5rem;
}

.review-comment {
  color: #444;
  line-height: 1.6;
  margin-bottom: 1rem;
}

.no-comment {
  font-style: italic;
  color: var(--secondary-color);
  margin-bottom: 1rem;
}

.reviewer-info {
  padding: 0.75rem;
  background: var(--light-gray);
  border-radius: 8px;
  font-size: 0.9rem;
}

.card-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--border-color);
}

.approve-btn, .reject-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
}

.approve-btn {
  background: #28a745;
  color: #fff;
  border: none;
}

.approve-btn:hover {
  background: #218838;
}

.reject-btn {
  background: #fff;
  color: #dc3545;
  border: 1px solid #dc3545;
}

.reject-btn:hover {
  background: #ffe6e6;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: #fff;
  border-radius: 12px;
}

.empty-state i {
  font-size: 3rem;
  color: #28a745;
  margin-bottom: 1rem;
}

.empty-state p {
  color: var(--secondary-color);
}
</style>
