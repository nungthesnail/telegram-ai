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
          <textarea v-model="newMessage" rows="3" placeholder="Введите сообщение..."></textarea>
          <button @click="sendMessage" :disabled="!newMessage || isLoading" class="btn btn-primary">
            Отправить
          </button>
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

const loadDialog = async () => {
  dialog.value = await authStore.apiFetch(`/api/dialogs/${route.params.dialogId}`)
  console.log(dialog.value)
}

const sendMessage = async () => {
  if (!newMessage.value) return
  
  try {
    isLoading.value = true
    messages.value.push({id: '', sender: 1, content: newMessage.value, createdAtUtc: Date.now() })
    const msgText = newMessage.value
    newMessage.value = ''

    const response = await authStore.apiFetch(`/api/dialogs/${dialog.value.id}/messages`, {
      method: 'POST',
      body: JSON.stringify({ message: msgText })
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
    messages.value = d.messages || []
  } catch (error) {
    console.error('Failed to load messages:', error)
  }
}

onMounted(async () => {
  await loadDialog()
  await loadMessages()
})
</script>

<style scoped>
.dialog-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.dialog-header {
  margin-bottom: 1.5rem;
}

.btn-back {
  display: inline-block;
  margin-bottom: 1rem;
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.btn-back:hover {
  color: #5568d3;
}

.chat-container {
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  height: 600px;
  display: flex;
  flex-direction: column;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
}

.input-area {
  padding: 1rem;
  border-top: 1px solid #eee;
  display: flex;
  gap: 1rem;
}

.input-area textarea {
  flex: 1;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.25rem;
  resize: none;
}

.btn {
  padding: 0.75rem 1.5rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.25rem;
  cursor: pointer;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>

