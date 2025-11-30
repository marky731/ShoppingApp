<script setup>
import { ref, onMounted } from 'vue'
import { adminAPI } from '../../services/api'

const users = ref([])
const loading = ref(true)
const error = ref(null)
const page = ref(1)
const totalPages = ref(1)
const roleFilter = ref('')
const searchQuery = ref('')

const roles = ['customer', 'seller', 'admin']

onMounted(async () => {
  await loadUsers()
})

const loadUsers = async () => {
  loading.value = true
  try {
    const params = { page: page.value, pageSize: 20 }
    if (roleFilter.value) params.role = roleFilter.value
    if (searchQuery.value) params.search = searchQuery.value

    const response = await adminAPI.getUsers(params)
    users.value = response.data.items || []
    totalPages.value = Math.ceil(response.data.totalCount / 20) || 1
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to load users'
  } finally {
    loading.value = false
  }
}

const suspendUser = async (userId) => {
  if (!confirm('Are you sure you want to suspend this user?')) return
  try {
    await adminAPI.suspendUser(userId)
    await loadUsers()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to suspend user'
  }
}

const activateUser = async (userId) => {
  try {
    await adminAPI.activateUser(userId)
    await loadUsers()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to activate user'
  }
}

const deleteUser = async (userId) => {
  if (!confirm('Are you sure you want to delete this user? This cannot be undone.')) return
  try {
    await adminAPI.deleteUser(userId)
    await loadUsers()
  } catch (err) {
    error.value = err.response?.data?.message || 'Failed to delete user'
  }
}

const formatDate = (date) => new Date(date).toLocaleDateString('en-US', {
  year: 'numeric', month: 'short', day: 'numeric'
})

const handleSearch = () => {
  page.value = 1
  loadUsers()
}
</script>

<template>
  <div class="admin-users">
    <div class="page-header">
      <h1>User Management</h1>
      <div class="filters">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Search users..."
          @keyup.enter="handleSearch"
        />
        <select v-model="roleFilter" @change="page = 1; loadUsers()">
          <option value="">All Roles</option>
          <option v-for="r in roles" :key="r" :value="r">{{ r }}</option>
        </select>
      </div>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>

    <div v-if="loading" class="loading">Loading users...</div>

    <div v-else-if="users.length === 0" class="empty-state">
      <i class="bi bi-people"></i>
      <p>No users found.</p>
    </div>

    <table v-else class="users-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Name</th>
          <th>Email</th>
          <th>Role</th>
          <th>Shop</th>
          <th>Status</th>
          <th>Joined</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="user in users" :key="user.userId">
          <td>{{ user.userId }}</td>
          <td>{{ user.firstName }} {{ user.lastName }}</td>
          <td>{{ user.email }}</td>
          <td>
            <span :class="['role-badge', user.role]">{{ user.role }}</span>
          </td>
          <td>
            <span v-if="user.shop">
              {{ user.shop.shopName }}
              <span :class="['shop-status', user.shop.isApproved ? 'approved' : 'pending']">
                ({{ user.shop.isApproved ? 'Approved' : 'Pending' }})
              </span>
            </span>
            <span v-else class="no-shop">-</span>
          </td>
          <td>
            <span :class="['status-badge', user.isActive ? 'active' : 'suspended']">
              {{ user.isActive ? 'Active' : 'Suspended' }}
            </span>
          </td>
          <td>{{ formatDate(user.createdAt) }}</td>
          <td>
            <button
              v-if="user.isActive && user.role !== 'admin'"
              class="action-btn suspend"
              @click="suspendUser(user.userId)"
              title="Suspend"
            >
              <i class="bi bi-pause-circle"></i>
            </button>
            <button
              v-if="!user.isActive"
              class="action-btn activate"
              @click="activateUser(user.userId)"
              title="Activate"
            >
              <i class="bi bi-play-circle"></i>
            </button>
            <button
              v-if="user.role !== 'admin'"
              class="action-btn delete"
              @click="deleteUser(user.userId)"
              title="Delete"
            >
              <i class="bi bi-trash"></i>
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="totalPages > 1" class="pagination">
      <button @click="page--; loadUsers()" :disabled="page === 1">Previous</button>
      <span>Page {{ page }} of {{ totalPages }}</span>
      <button @click="page++; loadUsers()" :disabled="page === totalPages">Next</button>
    </div>
  </div>
</template>

<style scoped>
.admin-users h1 {
  font-weight: 400;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  flex-wrap: wrap;
  gap: 1rem;
}

.filters {
  display: flex;
  gap: 1rem;
}

.filters input, .filters select {
  padding: 0.5rem 1rem;
  border: 1px solid var(--border-color);
  border-radius: 8px;
}

.filters input {
  width: 200px;
}

.users-table {
  width: 100%;
  background: #fff;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  border-collapse: collapse;
}

.users-table th, .users-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid var(--border-color);
}

.users-table th {
  background: var(--light-gray);
  font-weight: 500;
}

.role-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  text-transform: capitalize;
}

.role-badge.admin { background: #d4edda; color: #155724; }
.role-badge.seller { background: #cce5ff; color: #004085; }
.role-badge.customer { background: var(--light-gray); color: var(--secondary-color); }

.shop-status {
  font-size: 0.8rem;
}

.shop-status.approved { color: #155724; }
.shop-status.pending { color: #856404; }

.no-shop { color: var(--secondary-color); }

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
}

.status-badge.active { background: #d4edda; color: #155724; }
.status-badge.suspended { background: #f8d7da; color: #721c24; }

.action-btn {
  background: none;
  border: 1px solid var(--border-color);
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  margin-right: 0.25rem;
}

.action-btn:hover { background: var(--light-gray); }
.action-btn.suspend { color: #856404; }
.action-btn.activate { color: #155724; }
.action-btn.delete { color: #dc3545; }
.action-btn.delete:hover { background: #ffe6e6; }

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
