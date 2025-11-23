<template>
  <div class="subscription-page">
    <div class="container">
      <h1>Оплата подписки</h1>
      <div class="subscription-card">
        <h2>Текущий статус</h2>
        <p>Статус: {{ authStore.user?.subscriptionStatus || 'Неизвестно' }}</p>
        <p v-if="authStore.user?.subscriptionExpiresAtUtc">
          Истекает: {{ formatDate(authStore.user.subscriptionExpiresAtUtc) }}
        </p>
        <button @click="startSubscription" class="btn btn-primary">Оформить подписку</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('ru-RU')
}

const startSubscription = async () => {
  try {
    await authStore.apiFetch('/api/subscription/start', {
      method: 'POST',
      body: JSON.stringify({
        PlanCode: "0",
        TrialDays: 7,
        Amount: 10,
        Currency: "RUB"
      })
    })
    alert('Подписка оформлена')
  } catch (error) {
    console.error('Failed to start subscription:', error)
  }
}
</script>

<style scoped>
.subscription-page {
  padding: 2rem;
  max-width: 800px;
  margin: 0 auto;
}

.subscription-card {
  background: white;
  padding: 2rem;
  border-radius: 0.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.btn {
  padding: 0.75rem 1.5rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.25rem;
  cursor: pointer;
  margin-top: 1rem;
}
</style>

