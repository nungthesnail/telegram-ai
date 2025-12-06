<template>
  <div class="dialog-page">
    <div class="container">
      <div class="dialog-header">
        <router-link to="/channels" class="btn-back">← Назад к каналам</router-link>
        <h1>Диалог с ассистентом</h1>
      </div>
      <div class="chat-container">
        <div class="messages">
          <div v-for="message in messages" :key="message.id">
            <DialogMessage :message="message" :channel-id="dialog.channelId"></DialogMessage>
          </div>
        </div>
        <div class="input-area">
          <div class="input-controls">
            <select v-model="selectedModelId" class="model-select">
              <option value="" disabled>Выберите модель</option>
              <option v-for="model in models" :key="model.id" :value="model.id">
                {{ model.name }}
              </option>
            </select>
            <textarea v-model="newMessage" rows="3" placeholder="Введите сообщение..."></textarea>
            <button @click="sendMessage" :disabled="!newMessage || selectedModelId === null || isLoading" class="btn btn-primary">
              Отправить
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import DialogMessage from '../components/DialogMessage.vue'

const route = useRoute()
const authStore = useAuthStore()
const messages = ref([])
const newMessage = ref('')
const isLoading = ref(false)
const dialog = ref(null)
const models = ref([])
const selectedModelId = ref(null)

const loadDialog = async () => {
  dialog.value = await authStore.apiFetch(`/api/dialogs/${route.params.dialogId}`)
}

const sendMessage = async () => {
  if (!newMessage.value) return
  
  try {
    isLoading.value = true
    messages.value.push({
      id: '',
      sender: 1,
      entities: [{'$type': 'text', text: newMessage.value}],
      createdAtUtc: Date.now()
    })
    const msgText = newMessage.value
    newMessage.value = ''

    const response = await authStore.apiFetch(`/api/dialogs/${dialog.value.id}/messages`, {
      method: 'POST',
      body: JSON.stringify({ message: msgText, modelId: selectedModelId.value })
    })
    
    messages.value.pop()
    messages.value.push(response.userMessage)
    messages.value.push(response.assistantResponse)
    
    // Scroll to bottom
    const messagesContainer = document.querySelector('.messages')
    if (messagesContainer) {
      messagesContainer.scrollTop = messagesContainer.scrollHeight
    }
  } catch (error) {
    console.error('Failed to send message:', error)
  } finally {
    isLoading.value = false
  }
}

const loadMessages = async () => {
  try {
    const d = await authStore.apiFetch(`/api/dialogs/${dialog.value.id}`)
    messages.value = d.messages.filter((x) => x.sender === 1 || x.sender === 2) || []
  } catch (error) {
    console.error('Failed to load messages:', error)
  }
}

const loadModels = async () => {
  try {
    models.value = await authStore.apiFetch('/api/llm-models')
    if (models.value.length > 0 && !selectedModelId.value) {
      selectedModelId.value = models.value[0].id
    }
  } catch (error) {
    console.error('Failed to load models:', error)
  }
}

onMounted(async () => {
  await loadModels()
  await loadDialog()
  await loadMessages()
})
</script>

<style scoped>
/* Все стили перенесены в style.css */
</style>

