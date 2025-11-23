<template>
  <div class="dialog-page">
    <div class="container">
      <div class="dialog-header">
        <router-link to="/channels" class="btn-back">← Назад к каналам</router-link>
        <h1>Диалог с ассистентом</h1>
      </div>
      <div class="chat-container">
        <div class="messages">
          <div v-for="message in messages" :key="message.id" :class="['message', 'sender-' + message.sender]">
            <div class="message-meta">
              <strong>{{ message.sender === 1 ? 'Вы' : 'Ассистент' }}</strong>
              <small>{{ formatDate(message.createdAtUtc) }}</small>
            </div>
            <div class="message-content">
              <p>{{ message.content }}</p>
            </div>
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

const route = useRoute()
const authStore = useAuthStore()
const messages = ref([])
const newMessage = ref('')
const isLoading = ref(false)

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('ru-RU')
}

const sendMessage = async () => {
  if (!newMessage.value) return
  
  try {
    isLoading.value = true
    const dialogId = route.params.dialogId

    messages.value.push({id: '', sender: 1, content: newMessage, createdAtUtc: Date.now() })
    const response = await authStore.apiFetch(`/api/dialogs/${dialogId}/messages`, {
      method: 'POST',
      body: JSON.stringify({ message: newMessage.value })
    })
    
    // Reload messages to get assistant response
    messages.value.pop()
    messages.value.push(response.userMessage)
    messages.value.push(response.assistantResponse)
    newMessage.value = ''
    
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
    const dialogId = route.params.dialogId
    const dialog = await authStore.apiFetch(`/api/dialogs/${dialogId}`)
    messages.value = dialog.messages || []
  } catch (error) {
    console.error('Failed to load messages:', error)
  }
}

onMounted(() => {
  loadMessages()
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

.message {
  margin-bottom: 0.75rem;
  display: flex;
  width: 100%;
  align-items: flex-start;
  gap: 0.5rem;
}

.message.sender-1 {
  justify-content: flex-end;
}

.message.sender-2 {
  justify-content: flex-start;
}

.message-content {
  display: inline-block;
  max-width: 80%;
  padding: 0.5rem 0.75rem;
  border-radius: 0.5rem;
  position: relative;
}

.message.sender-1 .message-content {
  background: #e3f2fd;
  text-align: right;
}

.message.sender-2 .message-content {
  background: #f5f5f5;
  text-align: left;
}

.message-content p {
  margin: 0;
  font-size: 0.9rem;
  line-height: 1.4;
  word-wrap: break-word;
}

.message-meta {
  display: none;
  flex-shrink: 0;
  font-size: 0.75rem;
  opacity: 0.7;
  white-space: nowrap;
  padding-top: 0.5rem;
}

.message.sender-1 .message-meta {
  order: 0;
}

.message.sender-1 .message-content {
  order: 1;
}

.message.sender-2 .message-meta {
  order: 1;
}

.message.sender-2 .message-content {
  order: 0;
}

.message:hover .message-meta {
  display: flex;
  flex-direction: column;
}

.message.sender-1:hover .message-meta {
  align-items: flex-end;
}

.message.sender-2:hover .message-meta {
  align-items: flex-start;
}

.message-meta strong {
  color: #667eea;
  display: block;
  margin-bottom: 0.125rem;
}

.message-meta small {
  color: #999;
  display: block;
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

