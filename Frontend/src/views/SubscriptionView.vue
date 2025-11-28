<template>
  <div class="subscription-page">
    <div class="container">
      <h1>Оплата подписки</h1>
      
      <!-- Если подписка активна, перенаправляем на settings -->
      <div v-if="hasActiveSubscription" class="subscription-card">
        <h2>Подписка активна</h2>
        <p>Ваша подписка активна до: {{ formatDate(subscription?.expiresAtUtc) }}</p>
        <p>План: {{ subscription?.plan?.name }}</p>
        <router-link to="/settings" class="btn btn-primary">Перейти в настройки</router-link>
      </div>

      <!-- Если подписка неактивна, показываем планы -->
      <div v-else>
        <div v-if="loading" class="subscription-card">
          <p>Загрузка планов...</p>
        </div>
        <div v-else-if="plans.length === 0" class="subscription-card">
          <p>Планы подписки временно недоступны</p>
        </div>
        <div v-else>
          <div class="plans-grid">
            <div v-for="plan in plans" :key="plan.id" class="plan-card">
              <h3>{{ plan.name }}</h3>
              <p class="plan-description">{{ plan.description }}</p>
              <div class="plan-price">
                <span class="price-amount">{{ plan.priceRub }} ₽</span>
                <span class="price-period">за {{ plan.periodDays }} {{ getPeriodText(plan.periodDays) }}</span>
              </div>
              <div class="plan-features">
                <p><strong>{{ plan.tokensPerPeriod.toLocaleString() }}</strong> токенов</p>
              </div>
              <button @click="openPaymentModal(plan)" class="btn btn-primary">Оформить</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Модальное окно выбора способа оплаты -->
      <div v-if="selectedPlan" class="modal" @click.self="closePaymentModal">
        <div class="modal-content">
          <h2>Выберите способ оплаты</h2>
          <p>План: <strong>{{ selectedPlan.name }}</strong></p>
          <div class="payment-methods">
            <button @click="requestTelegramInvoice" class="payment-method-btn">
              <div class="payment-method-icon">📱</div>
              <div class="payment-method-info">
                <h4>Выставление счета в Telegram боте</h4>
                <p>Оплата через Telegram</p>
              </div>
            </button>
            <button @click="payWithCard" class="payment-method-btn" disabled>
              <div class="payment-method-icon">💳</div>
              <div class="payment-method-info">
                <h4>Оплата картой</h4>
                <p>Через YooKassa (скоро)</p>
              </div>
            </button>
          </div>
          <button @click="closePaymentModal" class="btn btn-secondary" style="margin-top: 1rem;">Отмена</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()
const plans = ref([])
const subscription = ref(null)
const loading = ref(true)
const selectedPlan = ref(null)

const hasActiveSubscription = computed(() => {
  if (!subscription.value) return false
  const now = new Date()
  const expiresAt = new Date(subscription.value.expiresAtUtc)
  return expiresAt > now
})

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('ru-RU')
}

const getPeriodText = (days) => {
  if (days === 30) return 'день'
  if (days === 7) return 'дней'
  if (days === 1) return 'день'
  const lastDigit = days % 10
  if (lastDigit === 1) return 'день'
  if (lastDigit >= 2 && lastDigit <= 4) return 'дня'
  return 'дней'
}

const loadPlans = async () => {
  try {
    plans.value = await authStore.apiFetch('/api/subscription/plans')
  } catch (error) {
    console.error('Failed to load plans:', error)
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

const openPaymentModal = (plan) => {
  selectedPlan.value = plan
}

const closePaymentModal = () => {
  selectedPlan.value = null
}

const requestTelegramInvoice = async () => {
  if (!selectedPlan.value) return
  
  try {
    await authStore.apiFetch('/api/subscription/request-telegram-invoice', {
      method: 'POST',
      body: JSON.stringify({
        planId: selectedPlan.value.id
      })
    })
    alert('Счет отправлен в Telegram бот. Проверьте сообщения от бота.')
    closePaymentModal()
  } catch (error) {
    console.error('Failed to request Telegram invoice:', error)
    alert('Не удалось отправить счет. Убедитесь, что ваш Telegram аккаунт подтвержден.')
  }
}

const payWithCard = () => {
  // Реализация оплаты картой через YooKassa будет добавлена позже
  alert('Оплата картой будет доступна в ближайшее время')
}

onMounted(async () => {
  loading.value = true
  await Promise.all([loadPlans(), loadSubscription()])
  loading.value = false
})

// Следим за изменением подписки и перенаправляем при необходимости
watch(hasActiveSubscription, (isActive) => {
  if (isActive) {
    router.push('/settings')
  }
}, { immediate: true })
</script>

<style scoped>
/* Все стили перенесены в style.css */
</style>

