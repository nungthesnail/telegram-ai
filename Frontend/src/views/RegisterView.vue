<template>
  <div class="register-page">
    <div class="register-card">
      <h1>Регистрация</h1>
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label>Email</label>
          <input v-model="email" type="email" required placeholder="your@email.com" />
        </div>
        <div class="form-group">
          <label>Имя</label>
          <input v-model="displayName" type="text" required placeholder="Ваше имя" />
        </div>
        <div class="form-group">
          <label>Пароль</label>
          <input v-model="password" type="password" required placeholder="Минимум 6 символов" minlength="6" />
        </div>
        <div class="form-group">
          <label>Подтвердите пароль</label>
          <input v-model="confirmPassword" type="password" required placeholder="Повторите пароль" />
        </div>
        <button type="submit" :disabled="isLoading" class="btn btn-primary">
          {{ isLoading ? 'Регистрация...' : 'Зарегистрироваться' }}
        </button>
        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        <p class="link-text">
          Уже есть аккаунт? <router-link to="/login">Войти</router-link>
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
const displayName = ref('')
const password = ref('')
const confirmPassword = ref('')
const isLoading = ref(false)
const errorMessage = ref('')

const handleRegister = async () => {
  if (password.value !== confirmPassword.value) {
    errorMessage.value = 'Пароли не совпадают'
    return
  }
  
  if (password.value.length < 6) {
    errorMessage.value = 'Пароль должен содержать минимум 6 символов'
    return
  }

  try {
    isLoading.value = true
    errorMessage.value = ''
    const API_URL = import.meta.env.VITE_API_URL ?? ''
    const response = await fetch(`${API_URL}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email: email.value, displayName: displayName.value, password: password.value })
    })
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Ошибка регистрации' }))
      throw new Error(errorData.message || 'Ошибка регистрации')
    }
    
    const data = await response.json()
    authStore.setToken(data.token)
    authStore.setUser(data.user)
    
    router.push('/channels')
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

