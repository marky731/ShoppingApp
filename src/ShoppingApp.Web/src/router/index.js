import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const routes = [
  {
    path: '/',
    name: 'home',
    component: HomeView
  },
  {
    path: '/products',
    name: 'products',
    component: () => import('../views/ProductsView.vue')
  },
  {
    path: '/products/:slug',
    name: 'product-detail',
    component: () => import('../views/ProductDetailView.vue')
  },
  {
    path: '/category/:id',
    name: 'category',
    component: () => import('../views/ProductsView.vue')
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('../views/LoginView.vue')
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('../views/RegisterView.vue')
  },
  {
    path: '/cart',
    name: 'cart',
    component: () => import('../views/CartView.vue')
  },
  {
    path: '/favorites',
    name: 'favorites',
    component: () => import('../views/FavoritesView.vue')
  },
  {
    path: '/checkout',
    name: 'checkout',
    component: () => import('../views/CheckoutView.vue')
  },
  {
    path: '/orders',
    name: 'orders',
    component: () => import('../views/OrdersView.vue')
  },
  {
    path: '/orders/:id',
    name: 'order-detail',
    component: () => import('../views/OrderDetailView.vue')
  },
  {
    path: '/shop/:id',
    name: 'shop',
    component: () => import('../views/ShopView.vue')
  },
  {
    path: '/profile',
    name: 'profile',
    component: () => import('../views/ProfileView.vue')
  },
  {
    path: '/seller',
    name: 'seller',
    component: () => import('../views/seller/SellerDashboard.vue'),
    children: [
      {
        path: 'shop',
        name: 'seller-shop',
        component: () => import('../views/seller/SellerShop.vue')
      },
      {
        path: 'products',
        name: 'seller-products',
        component: () => import('../views/seller/SellerProducts.vue')
      },
      {
        path: 'orders',
        name: 'seller-orders',
        component: () => import('../views/seller/SellerOrders.vue')
      },
      {
        path: 'discounts',
        name: 'seller-discounts',
        component: () => import('../views/seller/SellerDiscounts.vue')
      }
    ]
  },
  {
    path: '/admin',
    name: 'admin',
    component: () => import('../views/admin/AdminDashboard.vue'),
    children: [
      {
        path: 'users',
        name: 'admin-users',
        component: () => import('../views/admin/AdminUsers.vue')
      },
      {
        path: 'sellers',
        name: 'admin-sellers',
        component: () => import('../views/admin/AdminSellers.vue')
      },
      {
        path: 'categories',
        name: 'admin-categories',
        component: () => import('../views/admin/AdminCategories.vue')
      },
      {
        path: 'reviews',
        name: 'admin-reviews',
        component: () => import('../views/admin/AdminReviews.vue')
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
