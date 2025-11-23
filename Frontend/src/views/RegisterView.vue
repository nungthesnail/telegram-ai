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
.register-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
  padding: 2rem;
}

.register-card {
  background: white;
  padding: 2rem;
  border-radius: 0.5rem;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  width: 100%;
  max-width: 400px;
}

.register-card h1 {
  margin-bottom: 2rem;
  text-align: center;
  color: #333;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #666;
  font-weight: 500;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.25rem;
  font-size: 1rem;
}

.btn {
  width: 100%;
  padding: 0.75rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.25rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn:hover:not(:disabled) {
  background: #5568d3;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.error {
  color: #e74c3c;
  margin-top: 1rem;
  text-align: center;
}

.link-text {
  text-align: center;
  margin-top: 1.5rem;
  color: #666;
}

.link-text a {
  color: #667eea;
  text-decoration: none;
}
</style>

