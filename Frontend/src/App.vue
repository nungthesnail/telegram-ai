<template>
  <div id="app">
    <nav v-if="authStore.isAuthenticated.value" class="navbar">
      <div class="nav-container">
        <router-link to="/channels" class="nav-brand">Telegram AI Assistant</router-link>
        <div class="nav-links">
          <router-link to="/channels">Каналы</router-link>
          <router-link to="/settings">Настройки</router-link>
          <router-link to="/subscription">Подписка</router-link>
          <button @click="handleLogout" class="btn-logout">Выйти</button>
        </div>
      </div>
    </nav>
    <router-view />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

onMounted(async () => {
  if (authStore.isAuthenticated.value && !authStore.user) {
    try {
      const user = await authStore.apiFetch('/api/auth/me')
      authStore.setUser(user)
    } catch (error) {
      authStore.logout()
    }
  }
})
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
  color: #333;
  line-height: 1.6;
}

.navbar {
  background: white;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  padding: 1rem 0;
  margin-bottom: 2rem;
}

.nav-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-brand {
  font-size: 1.5rem;
  font-weight: bold;
  color: #667eea;
  text-decoration: none;
}

.nav-links {
  display: flex;
  gap: 2rem;
  align-items: center;
}

.nav-links a {
  color: #666;
  text-decoration: none;
  transition: color 0.2s;
}

.nav-links a:hover,
.nav-links a.router-link-active {
  color: #667eea;
}

.btn-logout {
  background: #e74c3c;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-logout:hover {
  background: #c0392b;
}
</style>
