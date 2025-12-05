<template>
  <div class="settings-page">
    <div class="container">
      <h1>Настройки профиля</h1>
      
      <!-- Информация о пользователе -->
      <div class="settings-card">
        <h2>Информация о пользователе</h2>
        <p><strong>Email:</strong> {{ authStore.user?.email }}</p>
        <p><strong>Имя:</strong> {{ authStore.user?.displayName }}</p>
        <p v-if="authStore.user?.telegramUserId" class="success">
          ✅ Telegram аккаунт подтвержден (ID: {{ authStore.user.telegramUserId }})
        </p>
        <div v-else>
          <p>Подтвердите ваш Telegram аккаунт, чтобы использовать все функции</p>
          <div v-if="verificationCode" class="code-block">
            <p>Код подтверждения: <strong>{{ verificationCode }}</strong></p>
            <p>Отправьте в боте команду: <code>/verify {{ verificationCode }}</code></p>
          </div>
          <button @click="generateCode" class="btn btn-primary">Получить код</button>
        </div>
      </div>

      <!-- Информация о подписке -->
      <div v-if="subscription" class="settings-card">
        <h2>Подписка</h2>
        <div class="subscription-info">
          <p><strong>План:</strong> {{ subscription.plan.name }}</p>
          <p><strong>Описание:</strong> {{ subscription.plan.description }}</p>
          <p><strong>Истекает:</strong> {{ formatDate(subscription.expiresAtUtc) }}</p>
          
          <div class="balance-section">
            <h3>Баланс токенов</h3>
            <div class="circular-progress-container">
              <CircularProgressBar 
                :percentage="balancePercentage" 
                :size="120" 
                :stroke-width="12" 
                color="#667eea" 
              />
              <div class="balance-text">
                <p class="balance-value">{{ balancePercentage.toFixed(1) }}%</p>
                <p class="balance-details">
                  {{ formatBalance(subscription.balance) }} / {{ formatBalance(subscription.lastReplenishAmount) }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div v-else class="settings-card">
        <h2>Подписка</h2>
        <p>У вас нет активной подписки</p>
        <router-link to="/subscription" class="btn btn-primary">Оформить подписку</router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import CircularProgressBar from '../components/CircularProgressBar.vue'

const authStore = useAuthStore()
const verificationCode = ref('')
const subscription = ref(null)

const balancePercentage = computed(() => {
  if (!subscription.value || subscription.value.lastReplenishAmount <= 0) {
    return 0
  }
  return (subscription.value.balance / subscription.value.lastReplenishAmount) * 100
})

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('ru-RU')
}

const formatBalance = (balance) => {
  if (balance == null) return '0'
  return balance.toFixed(2)
}

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

const loadSubscription = async () => {
  try {
    subscription.value = await authStore.apiFetch('/api/subscription/current')
  } catch (error) {
    if (error.message !== 'Subscription required') {
      console.error('Failed to load subscription:', error)
    }
    subscription.value = null
  }
}

onMounted(async () => {
  await loadSubscription()
})
</script>

<style scoped>
/* Все стили перенесены в style.css */
</style>

