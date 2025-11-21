<script setup>
import { ref, reactive, onMounted, computed } from 'vue'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

const ensureForm = reactive({ email: '', displayName: '' })
const user = ref(null)
const userId = ref(localStorage.getItem('userId') ?? '')
const channels = ref([])
const selectedChannelId = ref('')
const dialogs = ref([])
const posts = ref([])
const newChannel = reactive({ title: '', description: '', telegramLink: '', category: '' })
const newDialog = reactive({ title: '', systemPrompt: '' })
const selectedDialogId = ref('')
const newMessage = ref('')
const assistantResponse = ref('')
const verificationCode = ref('')
const botLink = ref(null)
const isLoading = ref(false)
const errorMessage = ref('')

const selectedChannel = computed(() => channels.value.find((c) => c.id === selectedChannelId.value) ?? null)
const selectedDialog = computed(() => dialogs.value.find((d) => d.id === selectedDialogId.value) ?? null)

const apiFetch = async (path, options = {}) => {
  const headers = { ...(options.headers ?? {}) }
  if (options.body && !headers['Content-Type']) {
    headers['Content-Type'] = 'application/json'
  }
  if (userId.value) {
    headers['X-User-Id'] = userId.value
  }

  const response = await fetch(`${API_URL}${path}`, { ...options, headers })
  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || 'Request failed')
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

const ensureUser = async () => {
  try {
    isLoading.value = true
    const created = await apiFetch('/api/users/ensure', {
      method: 'POST',
      body: JSON.stringify(ensureForm)
    })
    userId.value = created.id
    localStorage.setItem('userId', created.id)
    await loadUser()
    await loadChannels()
    errorMessage.value = ''
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    isLoading.value = false
  }
}

const loadUser = async () => {
  if (!userId.value) return
  user.value = await apiFetch('/api/users/me')
}

const loadChannels = async () => {
  if (!userId.value) return
  channels.value = await apiFetch('/api/channels')
  if (channels.value.length && !selectedChannelId.value) {
    await selectChannel(channels.value[0].id)
  }
}

const selectChannel = async (channelId) => {
  selectedChannelId.value = channelId
  await Promise.all([loadDialogs(channelId), loadPosts(channelId)])
  botLink.value = null
}

const createChannel = async () => {
  try {
    isLoading.value = true
    await apiFetch('/api/channels', {
      method: 'POST',
      body: JSON.stringify(newChannel)
    })
    Object.assign(newChannel, { title: '', description: '', telegramLink: '', category: '' })
    await loadChannels()
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    isLoading.value = false
  }
}

const requestBotLink = async () => {
  if (!selectedChannelId.value) return
  try {
    botLink.value = await apiFetch(`/api/channels/${selectedChannelId.value}/bot/request`, {
      method: 'POST'
    })
  } catch (error) {
    errorMessage.value = error.message
  }
}

const loadDialogs = async (channelId) => {
  dialogs.value = await apiFetch(`/api/channels/${channelId}/dialogs`)
  if (dialogs.value.length) {
    selectedDialogId.value = dialogs.value[0].id
  } else {
    selectedDialogId.value = ''
  }
}

const loadPosts = async (channelId) => {
  posts.value = await apiFetch(`/api/channels/${channelId}/posts`)
}

const startDialog = async () => {
  if (!selectedChannelId.value) return
  try {
    const dialog = await apiFetch('/api/dialogs', {
      method: 'POST',
      body: JSON.stringify({
        channelId: selectedChannelId.value,
        title: newDialog.title,
        systemPrompt: newDialog.systemPrompt
      })
    })
    dialogs.value.unshift(dialog)
    selectedDialogId.value = dialog.id
    Object.assign(newDialog, { title: '', systemPrompt: '' })
  } catch (error) {
    errorMessage.value = error.message
  }
}

const sendMessage = async () => {
  if (!selectedDialogId.value || !newMessage.value) return
  try {
    const response = await apiFetch(`/api/dialogs/${selectedDialogId.value}/messages`, {
      method: 'POST',
      body: JSON.stringify({ message: newMessage.value })
    })
    assistantResponse.value = response.assistantMessage
    newMessage.value = ''
    await loadDialogs(selectedChannelId.value)
  } catch (error) {
    errorMessage.value = error.message
  }
}

const formatDate = (value) => {
  if (!value) return '-'
  return new Date(value).toLocaleString()
}

onMounted(async () => {
  if (userId.value) {
    await loadUser()
    await loadChannels()
  }
})
</script>

<template>
  <main class="page">
    <header class="hero">
      <div>
        <h1>Telegram AI Assistant</h1>
        <p>Запускайте диалоги, управляйте каналами, публикуйте посты</p>
      </div>
      <div class="user-card" v-if="user">
        <p class="label">Текущий пользователь</p>
        <strong>{{ user.displayName }}</strong>
        <p>{{ user.email }}</p>
        <small>Статус подписки: {{ user.subscriptionStatus }}</small>
      </div>
    </header>

    <section class="card grid">
      <div>
        <h2>1. Регистрация</h2>
        <p>Создайте аккаунт чтобы получить UserId. Он сохранится в браузере.</p>
        <div class="form">
          <label>Email</label>
          <input v-model="ensureForm.email" type="email" placeholder="admin@channel.ru" />
          <label>Имя</label>
          <input v-model="ensureForm.displayName" type="text" placeholder="Имя администратора" />
          <button @click="ensureUser" :disabled="isLoading">Сохранить</button>
          <p class="muted" v-if="userId">UserId: {{ userId }}</p>
        </div>
      </div>
      <div>
        <h2>2. Новый канал</h2>
        <p>Создайте карточку канала и добавьте в него Telegram-бота.</p>
        <div class="form">
          <label>Название</label>
          <input v-model="newChannel.title" placeholder="Мой канал" />
          <label>Описание</label>
          <textarea v-model="newChannel.description" rows="2"></textarea>
          <label>Ссылка</label>
          <input v-model="newChannel.telegramLink" placeholder="https://t.me/..." />
          <label>Категория</label>
          <input v-model="newChannel.category" placeholder="Новости" />
          <button @click="createChannel" :disabled="isLoading">Создать</button>
        </div>
      </div>
    </section>

    <section class="card">
      <div class="section-header">
        <h2>Каналы</h2>
        <span v-if="selectedChannel">Активный канал: {{ selectedChannel.title }}</span>
      </div>

      <div class="channel-list">
        <button
          v-for="channel in channels"
          :key="channel.id"
          :class="['channel-chip', { active: selectedChannelId === channel.id }]"
          @click="selectChannel(channel.id)"
        >
          {{ channel.title }}
        </button>
      </div>

      <div class="grid">
        <div>
          <h3>Bot интеграция</h3>
          <p>Сгенерируйте код, добавьте бота в Telegram и отправьте /verify.</p>
          <button @click="requestBotLink" :disabled="!selectedChannelId">Сгенерировать код</button>
          <div v-if="botLink" class="code-block">
            <p>Код подтверждения: <strong>{{ botLink.verificationCode }}</strong></p>
            <p>Команда: <code>/verify {{ selectedChannelId }} {{ botLink.verificationCode }}</code></p>
          </div>
        </div>

        <div>
          <h3>Новый диалог</h3>
          <div class="form">
            <label>Заголовок</label>
            <input v-model="newDialog.title" placeholder="Контент-план" />
            <label>Системное сообщение</label>
            <textarea v-model="newDialog.systemPrompt" rows="2" placeholder="Дай советы..." />
            <button @click="startDialog" :disabled="!selectedChannelId">Создать диалог</button>
          </div>
        </div>
      </div>
    </section>

    <section class="card grid">
      <div>
        <h2>Диалоги</h2>
        <ul class="list">
          <li
            v-for="dialog in dialogs"
            :key="dialog.id"
            :class="{ active: selectedDialogId === dialog.id }"
            @click="selectedDialogId = dialog.id"
          >
            <strong>{{ dialog.title }}</strong>
            <small>{{ dialog.messages.at(-1)?.content ?? 'Нет сообщений' }}</small>
          </li>
        </ul>

        <div class="chat" v-if="selectedDialog">
          <div class="chat-messages">
            <div
              v-for="message in selectedDialog.messages"
              :key="message.id"
              :class="['bubble', message.sender.toLowerCase()]"
            >
              <small>{{ message.sender }}</small>
              <p>{{ message.content }}</p>
              <span>{{ formatDate(message.createdAtUtc) }}</span>
            </div>
          </div>

          <div class="form">
            <textarea v-model="newMessage" rows="3" placeholder="Сообщение ассистенту"></textarea>
            <button @click="sendMessage" :disabled="!newMessage">Отправить</button>
          </div>

          <div class="response" v-if="assistantResponse">
            <p>Ответ ассистента:</p>
            <blockquote>{{ assistantResponse }}</blockquote>
          </div>
        </div>
      </div>

      <div>
        <h2>Посты</h2>
        <ul class="list">
          <li v-for="post in posts" :key="post.id">
            <strong>{{ post.title || 'Без названия' }}</strong>
            <p>{{ post.content }}</p>
            <small>Статус: {{ post.status }} · Создан: {{ formatDate(post.createdAtUtc) }}</small>
          </li>
        </ul>
      </div>
    </section>

    <p class="error" v-if="errorMessage">Ошибка: {{ errorMessage }}</p>
  </main>
</template>
