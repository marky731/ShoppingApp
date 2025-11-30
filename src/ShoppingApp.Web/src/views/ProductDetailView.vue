<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { productsAPI, favoritesAPI, reviewsAPI } from '../services/api'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'

const route = useRoute()
const cartStore = useCartStore()
const authStore = useAuthStore()

const product = ref(null)
const loading = ref(true)
const quantity = ref(1)
const isFavorite = ref(false)
const favoriteLoading = ref(false)

// Reviews
const reviews = ref(null)
const reviewsLoading = ref(false)
const canReview = ref(false)
const eligibleOrderIds = ref([])
const showReviewForm = ref(false)
const reviewForm = ref({
  rating: 5,
  title: '',
  comment: '',
  orderId: null
})
const reviewSubmitting = ref(false)
const reviewError = ref(null)

const isAuthenticated = computed(() => authStore.isAuthenticated)

onMounted(async () => {
  await loadProduct()
})

watch(() => route.params.slug, async () => {
  await loadProduct()
})

const loadProduct = async () => {
  loading.value = true
  try {
    const response = await productsAPI.getBySlug(route.params.slug)
    product.value = response.data
    // Check if product is in favorites
    if (isAuthenticated.value && product.value) {
      await checkFavoriteStatus()
    }
    // Load reviews
    await loadReviews()
    // Check if user can review
    if (isAuthenticated.value && product.value) {
      await checkCanReview()
    }
  } catch (error) {
    console.error('Failed to load product:', error)
  } finally {
    loading.value = false
  }
}

const loadReviews = async () => {
  if (!product.value) return
  reviewsLoading.value = true
  try {
    const response = await reviewsAPI.getProductReviews(product.value.productId)
    reviews.value = response.data
  } catch (error) {
    console.error('Failed to load reviews:', error)
  } finally {
    reviewsLoading.value = false
  }
}

const checkCanReview = async () => {
  try {
    const response = await reviewsAPI.canReview(product.value.productId)
    canReview.value = response.data.canReview
    eligibleOrderIds.value = response.data.eligibleOrderIds
    if (eligibleOrderIds.value.length > 0) {
      reviewForm.value.orderId = eligibleOrderIds.value[0]
    }
  } catch (error) {
    console.error('Failed to check review eligibility:', error)
  }
}

const checkFavoriteStatus = async () => {
  try {
    const response = await favoritesAPI.check(product.value.productId)
    isFavorite.value = response.data.isFavorite
  } catch (error) {
    console.error('Failed to check favorite status:', error)
  }
}

const toggleFavorite = async () => {
  if (!isAuthenticated.value) {
    return
  }

  favoriteLoading.value = true
  try {
    if (isFavorite.value) {
      await favoritesAPI.remove(product.value.productId)
      isFavorite.value = false
    } else {
      await favoritesAPI.add(product.value.productId)
      isFavorite.value = true
    }
  } catch (error) {
    console.error('Failed to toggle favorite:', error)
  } finally {
    favoriteLoading.value = false
  }
}

const addToCart = () => {
  if (product.value) {
    cartStore.addItem(product.value, quantity.value)
  }
}

const submitReview = async () => {
  if (!reviewForm.value.rating || !reviewForm.value.orderId) {
    reviewError.value = 'Please select a rating and order'
    return
  }

  reviewSubmitting.value = true
  reviewError.value = null
  try {
    await reviewsAPI.create({
      productId: product.value.productId,
      orderId: reviewForm.value.orderId,
      rating: reviewForm.value.rating,
      title: reviewForm.value.title || null,
      comment: reviewForm.value.comment || null
    })
    showReviewForm.value = false
    reviewForm.value = { rating: 5, title: '', comment: '', orderId: eligibleOrderIds.value[0] }
    await loadReviews()
    await checkCanReview()
  } catch (error) {
    reviewError.value = error.response?.data?.message || 'Failed to submit review'
  } finally {
    reviewSubmitting.value = false
  }
}

const formatPrice = (price) => {
  return `$${price.toFixed(2)}`
}

const formatDate = (date) => {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const renderStars = (rating) => {
  return Array(5).fill(0).map((_, i) => i < rating ? 'bi-star-fill' : 'bi-star')
}
</script>

<template>
  <div class="page-container">
    <div v-if="loading" class="loading">
      Loading product...
    </div>

    <div v-else-if="product" class="product-detail">
      <div class="row">
        <div class="col-md-6">
          <div class="product-image-wrapper">
            <img
              :src="product.mainImageUrl || '/placeholder.png'"
              :alt="product.productName"
              class="img-fluid"
              style="max-height: 500px; object-fit: contain; width: 100%;"
            />
            <button
              v-if="isAuthenticated"
              class="favorite-btn-detail"
              :class="{ active: isFavorite }"
              @click="toggleFavorite"
              :disabled="favoriteLoading"
            >
              <i :class="isFavorite ? 'bi bi-heart-fill' : 'bi bi-heart'"></i>
            </button>
          </div>
        </div>
        <div class="col-md-6">
          <h1 style="font-weight: 300; margin-bottom: 1rem;">{{ product.productName }}</h1>

          <p style="font-size: 1.5rem; margin-bottom: 1rem;">
            {{ formatPrice(product.price) }}
          </p>

          <!-- Rating Display -->
          <div v-if="reviews && reviews.totalReviews > 0" class="product-rating">
            <span class="stars">
              <i v-for="(star, i) in renderStars(Math.round(reviews.averageRating))" :key="i" :class="['bi', star]"></i>
            </span>
            <span class="rating-text">{{ reviews.averageRating.toFixed(1) }} ({{ reviews.totalReviews }} reviews)</span>
          </div>

          <div v-if="product.brand" style="margin-bottom: 0.5rem;">
            <strong>Brand:</strong> {{ product.brand }}
          </div>

          <div v-if="product.shop" class="shop-link-container">
            Sold by:
            <router-link :to="`/shop/${product.shop.shopId}`" class="shop-link">
              {{ product.shop.shopName }}
            </router-link>
          </div>

          <p style="margin-bottom: 1.5rem; line-height: 1.6;">
            {{ product.description }}
          </p>

          <div style="display: flex; gap: 1rem; align-items: center; margin-bottom: 1rem;">
            <label>Quantity:</label>
            <input
              v-model.number="quantity"
              type="number"
              min="1"
              :max="product.stockQuantity"
              style="width: 60px; padding: 0.5rem; border: 1px solid #e0e0e0; border-radius: 4px;"
            />
          </div>

          <div style="display: flex; gap: 1rem; align-items: center;">
            <button class="add-to-cart-btn" style="padding: 1rem 2rem;" @click="addToCart">
              Add to Cart
            </button>
            <button
              v-if="isAuthenticated"
              class="favorite-btn-inline"
              :class="{ active: isFavorite }"
              @click="toggleFavorite"
              :disabled="favoriteLoading"
            >
              <i :class="isFavorite ? 'bi bi-heart-fill' : 'bi bi-heart'"></i>
              {{ isFavorite ? 'In Favorites' : 'Add to Favorites' }}
            </button>
          </div>

          <div v-if="product.stockQuantity < 10" style="margin-top: 1rem; color: #cc0000;">
            Only {{ product.stockQuantity }} left in stock!
          </div>
        </div>
      </div>

      <!-- Reviews Section -->
      <div class="reviews-section">
        <div class="reviews-header">
          <h2>Customer Reviews</h2>
          <button
            v-if="canReview"
            class="write-review-btn"
            @click="showReviewForm = true"
          >
            Write a Review
          </button>
        </div>

        <!-- Review Form Modal -->
        <div v-if="showReviewForm" class="review-form-overlay" @click.self="showReviewForm = false">
          <div class="review-form-modal">
            <h3>Write a Review</h3>

            <div v-if="reviewError" class="error-message">{{ reviewError }}</div>

            <div class="form-group">
              <label>Rating *</label>
              <div class="star-rating-input">
                <button
                  v-for="n in 5"
                  :key="n"
                  type="button"
                  @click="reviewForm.rating = n"
                  class="star-btn"
                  :class="{ active: n <= reviewForm.rating }"
                >
                  <i :class="n <= reviewForm.rating ? 'bi bi-star-fill' : 'bi bi-star'"></i>
                </button>
              </div>
            </div>

            <div class="form-group" v-if="eligibleOrderIds.length > 1">
              <label>For Order</label>
              <select v-model="reviewForm.orderId">
                <option v-for="orderId in eligibleOrderIds" :key="orderId" :value="orderId">
                  Order #{{ orderId }}
                </option>
              </select>
            </div>

            <div class="form-group">
              <label>Title (optional)</label>
              <input v-model="reviewForm.title" type="text" placeholder="Summarize your review" />
            </div>

            <div class="form-group">
              <label>Review (optional)</label>
              <textarea v-model="reviewForm.comment" rows="4" placeholder="Share your experience with this product"></textarea>
            </div>

            <div class="form-actions">
              <button class="cancel-btn" @click="showReviewForm = false">Cancel</button>
              <button class="submit-btn" @click="submitReview" :disabled="reviewSubmitting">
                {{ reviewSubmitting ? 'Submitting...' : 'Submit Review' }}
              </button>
            </div>
          </div>
        </div>

        <!-- Reviews Summary -->
        <div v-if="reviews && reviews.totalReviews > 0" class="reviews-summary">
          <div class="average-rating">
            <span class="big-rating">{{ reviews.averageRating.toFixed(1) }}</span>
            <div class="stars-large">
              <i v-for="(star, i) in renderStars(Math.round(reviews.averageRating))" :key="i" :class="['bi', star]"></i>
            </div>
            <span class="total-reviews">Based on {{ reviews.totalReviews }} reviews</span>
          </div>
          <div class="rating-bars">
            <div v-for="n in [5, 4, 3, 2, 1]" :key="n" class="rating-bar-row">
              <span class="bar-label">{{ n }} star</span>
              <div class="bar-container">
                <div
                  class="bar-fill"
                  :style="{ width: `${(reviews.ratingDistribution[n] / reviews.totalReviews) * 100}%` }"
                ></div>
              </div>
              <span class="bar-count">{{ reviews.ratingDistribution[n] || 0 }}</span>
            </div>
          </div>
        </div>

        <!-- Reviews List -->
        <div v-if="reviewsLoading" class="loading">Loading reviews...</div>

        <div v-else-if="reviews && reviews.reviews.length === 0" class="no-reviews">
          <p>No reviews yet. Be the first to review this product!</p>
        </div>

        <div v-else-if="reviews" class="reviews-list">
          <div v-for="review in reviews.reviews" :key="review.reviewId" class="review-card">
            <div class="review-header">
              <div class="review-stars">
                <i v-for="(star, i) in renderStars(review.rating)" :key="i" :class="['bi', star]"></i>
              </div>
              <span class="review-author">{{ review.userName }}</span>
              <span class="review-date">{{ formatDate(review.createdAt) }}</span>
            </div>
            <div v-if="review.title" class="review-title">{{ review.title }}</div>
            <p v-if="review.comment" class="review-comment">{{ review.comment }}</p>

            <!-- Seller Response -->
            <div v-if="review.response" class="seller-response">
              <div class="response-header">
                <i class="bi bi-reply"></i>
                <span>Response from {{ review.response.sellerName }}</span>
                <span class="response-date">{{ formatDate(review.response.createdAt) }}</span>
              </div>
              <p>{{ review.response.responseText }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="loading">
      Product not found.
    </div>
  </div>
</template>

<style scoped>
.product-image-wrapper {
  position: relative;
}

.favorite-btn-detail {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: #fff;
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  font-size: 1.2rem;
  transition: all 0.2s;
}

.favorite-btn-detail:hover {
  background: #f5f5f5;
}

.favorite-btn-detail.active {
  color: #ff4444;
}

.favorite-btn-detail:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.favorite-btn-inline {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: none;
  border: 1px solid #e0e0e0;
  padding: 1rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.favorite-btn-inline:hover {
  border-color: #000;
}

.favorite-btn-inline.active {
  color: #ff4444;
  border-color: #ff4444;
}

.favorite-btn-inline:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.shop-link-container {
  margin-bottom: 1rem;
  color: #666;
}

.shop-link {
  color: var(--primary-color);
  text-decoration: none;
  font-weight: 500;
}

.shop-link:hover {
  text-decoration: underline;
}

.product-rating {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.product-rating .stars {
  color: #ffc107;
}

.product-rating .rating-text {
  color: #666;
  font-size: 0.9rem;
}

/* Reviews Section */
.reviews-section {
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px solid var(--border-color);
}

.reviews-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.reviews-header h2 {
  font-weight: 400;
}

.write-review-btn {
  background: var(--primary-color);
  color: #fff;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
}

.write-review-btn:hover {
  background: var(--secondary-color);
}

.reviews-summary {
  display: flex;
  gap: 3rem;
  padding: 1.5rem;
  background: var(--light-gray);
  border-radius: 8px;
  margin-bottom: 2rem;
}

.average-rating {
  text-align: center;
}

.big-rating {
  font-size: 3rem;
  font-weight: 300;
}

.stars-large {
  color: #ffc107;
  font-size: 1.2rem;
  margin: 0.5rem 0;
}

.total-reviews {
  font-size: 0.9rem;
  color: #666;
}

.rating-bars {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.rating-bar-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.bar-label {
  width: 50px;
  font-size: 0.85rem;
  color: #666;
}

.bar-container {
  flex: 1;
  height: 8px;
  background: #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
}

.bar-fill {
  height: 100%;
  background: #ffc107;
  border-radius: 4px;
}

.bar-count {
  width: 30px;
  font-size: 0.85rem;
  color: #666;
  text-align: right;
}

.reviews-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.review-card {
  padding: 1.5rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}

.review-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.75rem;
}

.review-stars {
  color: #ffc107;
}

.review-author {
  font-weight: 500;
}

.review-date {
  color: #666;
  font-size: 0.85rem;
}

.review-title {
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.review-comment {
  color: #444;
  line-height: 1.6;
}

.seller-response {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--light-gray);
  border-radius: 8px;
}

.response-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  color: #666;
}

.response-date {
  margin-left: auto;
}

.no-reviews {
  text-align: center;
  padding: 2rem;
  color: #666;
}

/* Review Form Modal */
.review-form-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.review-form-modal {
  background: #fff;
  padding: 2rem;
  border-radius: 12px;
  width: 90%;
  max-width: 500px;
}

.review-form-modal h3 {
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

.form-group input,
.form-group textarea,
.form-group select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
  font-size: 1rem;
}

.star-rating-input {
  display: flex;
  gap: 0.25rem;
}

.star-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: #ddd;
  cursor: pointer;
  padding: 0.25rem;
}

.star-btn.active {
  color: #ffc107;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

.cancel-btn,
.submit-btn {
  flex: 1;
  padding: 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
}

.cancel-btn {
  background: #fff;
  border: 1px solid var(--border-color);
}

.submit-btn {
  background: var(--primary-color);
  color: #fff;
  border: none;
}

.submit-btn:hover {
  background: var(--secondary-color);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
</style>
