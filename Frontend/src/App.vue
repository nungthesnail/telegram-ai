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
/* Все стили перенесены в style.css */
</style>
