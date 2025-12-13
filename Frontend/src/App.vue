<template>
  <div id="app" :data-theme="theme">
    <!-- Мобильная кнопка меню -->
    <button 
      v-if="showSidebar" 
      @click="toggleMobileMenu" 
      class="mobile-menu-btn"
      :class="{ active: mobileMenuOpen }"
    >
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <path v-if="!mobileMenuOpen" d="M3 12h18M3 6h18M3 18h18"/>
        <path v-else d="M18 6L6 18M6 6l12 12"/>
      </svg>
    </button>

    <!-- Боковая навигация -->
    <nav 
      v-if="showSidebar" 
      class="sidebar"
      :class="{ open: mobileMenuOpen }"
    >
      <div class="sidebar-header">
        <router-link to="/channels" class="nav-brand" @click="closeMobileMenu">
          <span class="brand-icon">🤖</span>
          <span class="brand-text">Telegram AI</span>
        </router-link>
      </div>
      <div class="sidebar-content">
        <router-link to="/channels" @click="closeMobileMenu" class="nav-link">
          <span>💬 Каналы и диалоги</span>
        </router-link>
        <router-link to="/settings" @click="closeMobileMenu" class="nav-link">
          <span>⚙️ Настройки</span>
        </router-link>
        <router-link to="/subscription" @click="closeMobileMenu" class="nav-link">
          <span>💲Подписка</span>
        </router-link>
      </div>
      <div class="sidebar-footer">
        <button @click="toggleTheme" class="theme-toggle" :title="theme === 'dark' ? 'Светлая тема' : 'Темная тема'">
          <svg v-if="theme === 'light'" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
          </svg>
          <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="5"/>
            <path d="M12 1v2m0 18v2M4.22 4.22l1.42 1.42m12.72 12.72l1.42 1.42M1 12h2m18 0h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/>
          </svg>
        </button>
        <button @click="handleLogout" class="nav-link logout-btn">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
            <polyline points="16 17 21 12 16 7"/>
            <line x1="21" y1="12" x2="9" y2="12"/>
          </svg>
          <span>Выйти</span>
        </button>
      </div>
    </nav>

    <!-- Overlay для мобильного меню -->
    <div 
      v-if="showSidebar && mobileMenuOpen" 
      class="sidebar-overlay"
      @click="closeMobileMenu"
    ></div>

    <!-- Основной контент -->
    <div class="main-content" :class="{ 'no-sidebar': pageWithoutMenu }">
      <router-view />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from './stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const mobileMenuOpen = ref(false)
const theme = ref(localStorage.getItem('theme') || 'light')

const pageWithoutMenu = computed(() => route.path === '/' 
  || route.name === 'Login' 
  || route.name === 'Register')

const showSidebar = computed(() => authStore.isAuthenticated.value && !pageWithoutMenu.value)

const toggleMobileMenu = () => {
  mobileMenuOpen.value = !mobileMenuOpen.value
}

const closeMobileMenu = () => {
  mobileMenuOpen.value = false
}

const toggleTheme = () => {
  theme.value = theme.value === 'light' ? 'dark' : 'light'
  localStorage.setItem('theme', theme.value)
  document.documentElement.setAttribute('data-theme', theme.value)
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
  closeMobileMenu()
}

// Применяем тему при загрузке
onMounted(async () => {
  document.documentElement.setAttribute('data-theme', theme.value)
  
  if (authStore.isAuthenticated.value && !authStore.user) {
    try {
      const user = await authStore.apiFetch('/api/auth/me')
      authStore.setUser(user)
    } catch (error) {
      authStore.logout()
    }
  }
})

// Следим за изменением темы
watch(theme, (newTheme) => {
  document.documentElement.setAttribute('data-theme', newTheme)
})
</script>

<style>
/* Все стили перенесены в style.css */
</style>
