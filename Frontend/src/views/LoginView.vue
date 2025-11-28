<template>
  <div class="login-page">
    <div class="login-card">
      <h1>Вход</h1>
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label>Email</label>
          <input v-model="email" type="email" required placeholder="your@email.com" />
        </div>
        <div class="form-group">
          <label>Пароль</label>
          <input v-model="password" type="password" required placeholder="Введите пароль" />
        </div>
        <button type="submit" :disabled="isLoading" class="btn btn-primary">
          {{ isLoading ? 'Вход...' : 'Войти' }}
        </button>
        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        <p class="link-text">
          Нет аккаунта? <router-link to="/register">Зарегистрироваться</router-link>
        </p>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const isLoading = ref(false)
const errorMessage = ref('')

const handleLogin = async () => {
  try {
    isLoading.value = true
    errorMessage.value = ''
    const API_URL = import.meta.env.VITE_API_URL ?? ''
    const response = await fetch(`${API_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.value, password: password.value })
    })
    
    if (!response.ok) {
      throw new Error('Ошибка входа')
    }
    
    const data = await response.json()
    authStore.setToken(data.token)
    authStore.setUser(data.user)
    
    const redirect = router.currentRoute.value.query.redirect || '/channels'
    router.push(redirect)
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
/* Все стили перенесены в style.css */
</style>

