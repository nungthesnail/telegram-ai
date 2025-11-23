<template>
  <div class="settings-page">
    <div class="container">
      <h1>Настройки профиля</h1>
      <div class="settings-card">
        <h2>Подтверждение Telegram аккаунта</h2>
        <p v-if="!authStore.user?.telegramUserId">
          Подтвердите ваш Telegram аккаунт, чтобы использовать все функции
        </p>
        <p v-else class="success">
          ✅ Telegram аккаунт подтвержден (ID: {{ authStore.user.telegramUserId }})
        </p>
        <div v-if="verificationCode" class="code-block">
          <p>Код подтверждения: <strong>{{ verificationCode }}</strong></p>
          <p>Отправьте в боте команду: <code>/verify {{ verificationCode }}</code></p>
        </div>
        <button @click="generateCode" class="btn btn-primary">Получить код</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const verificationCode = ref('')

const generateCode = async () => {
  try {
    const response = await authStore.apiFetch('/api/users/verification-code', {
      method: 'POST'
    })
    verificationCode.value = response.verificationCode
  } catch (error) {
    console.error('Failed to generate code:', error)
  }
}
</script>

<style scoped>
.settings-page {
  padding: 2rem;
  max-width: 800px;
  margin: 0 auto;
}

.settings-card {
  background: white;
  padding: 2rem;
  border-radius: 0.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
}

.settings-card h2 {
  margin-bottom: 1rem;
}

.code-block {
  background: #f5f5f5;
  padding: 1rem;
  border-radius: 0.25rem;
  margin: 1rem 0;
}

.code-block code {
  background: #e0e0e0;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-family: monospace;
}

.success {
  color: #27ae60;
  font-weight: 500;
}

.btn {
  padding: 0.75rem 1.5rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.25rem;
  cursor: pointer;
}
</style>

