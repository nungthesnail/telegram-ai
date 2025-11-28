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
/* Все стили перенесены в style.css */
</style>

