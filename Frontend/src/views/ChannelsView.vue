<template>
  <div class="channels-page">
    <div class="container">
      <div class="header">
        <h1>Мои каналы и диалоги</h1>
        <button @click="showHelpModal = true" class="btn btn-secondary">Как добавить канал</button>
      </div>

      <div v-if="pendingChannels.length > 0" class="pending-channels">
        <h2>Каналы, ожидающие подтверждения</h2>
        <div v-for="channel in pendingChannels" :key="channel.id" class="channel-card pending">
          <div class="card-header">
            <div class="card-content">
              <h3>{{ channel.title || 'Без названия' }}</h3>
              <p>{{ channel.description || 'Нет описания' }}</p>
            </div>
            <div class="card-menu">
              <button @click.stop="openChannelMenu(channel.id)" class="menu-button" :class="{ active: openMenuId === channel.id }">
                ⋮
              </button>
              <div v-if="openMenuId === channel.id" class="menu-dropdown" @click.stop>
                <button @click="showDeleteChannelConfirm(channel.id)" class="menu-item delete">Удалить</button>
              </div>
            </div>
          </div>
          <button @click="confirmChannel(channel.id)" class="btn btn-primary">Подтвердить</button>
        </div>
      </div>

      <div class="channels-grid">
        <div v-for="channel in confirmedChannels" :key="channel.id" class="channel-card">
          <div class="card-header">
            <div class="card-content">
              <h3>{{ channel.title || 'Без названия' }}</h3>
              <p>{{ channel.description || 'Нет описания' }}</p>
            </div>
            <div class="card-menu">
              <button @click.stop="openChannelMenu(channel.id)" class="menu-button" :class="{ active: openMenuId === channel.id }">
                ⋮
              </button>
              <div v-if="openMenuId === channel.id" class="menu-dropdown" @click.stop>
                <button @click="showDeleteChannelConfirm(channel.id)" class="menu-item delete">Удалить</button>
              </div>
            </div>
          </div>
          <div class="channel-actions">
            <button @click="createDialog(channel.id)" class="btn btn-primary m-0-25" :disabled="isLoading">
              Новый диалог
            </button>
            <button @click="gotoPosts(channel.id)" class="btn btn-primary m-0-25">
              Посты
            </button>
          </div>
        </div>
      </div>

      <div v-if="dialogs.length > 0" class="dialogs-section">
        <h2>Диалоги</h2>
        <div class="dialogs-list">
          <div v-for="dialog in dialogs" :key="dialog.id" class="dialog-card">
            <div class="dialog-info">
              <h4>{{ dialog.title }}</h4>
              <p class="dialog-channel">Канал: {{ getChannelName(dialog.channelId) }}</p>
              <p class="dialog-preview">{{ getLastMessage(dialog) }}</p>
            </div>
            <div class="dialog-actions">
              <router-link :to="`/dialog/${dialog.id}`" class="btn btn-primary">
                Открыть
              </router-link>
              <div class="card-menu">
                <button @click.stop="openDialogMenu(dialog.id)" class="menu-button" :class="{ active: openMenuId === dialog.id }">
                  ⋮
                </button>
                <div v-if="openMenuId === dialog.id" class="menu-dropdown" @click.stop>
                  <button @click="showDeleteDialogConfirm(dialog.id)" class="menu-item delete">Удалить</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-if="showHelpModal" class="modal" @click.self="showHelpModal = false">
        <div class="modal-content">
          <h2>Как добавить канал</h2>
          <div class="help-content">
            <ol>
              <li>
                <strong>Подтвердите ваш Telegram аккаунт</strong>
                <p>Перейдите в настройки профиля и получите код подтверждения. Отправьте его боту командой<br><code>/verify &lt;код&gt;</code></p>
              </li>
              <li>
                <strong>Добавьте бота в ваш Telegram-канал</strong>
                <p>В настройках канала добавьте бота как администратора</p>
              </li>
              <li>
                <strong>Дождитесь, когда канал появится в веб-интерфейсе</strong>
                <p>После добавления бота в Telegram придет уведомление и канал добавится в ваш список</p>
              </li>
              <li>
                <strong>Создайте новый диалог</strong>
                <p>После подтверждения вы сможете создать диалог с ассистентом, который поможет вести канал</p>
              </li>
            </ol>
          </div>
          <button @click="showHelpModal = false" class="btn btn-primary">Понятно</button>
        </div>
      </div>

      <!-- Модальное окно подтверждения удаления канала -->
      <div v-if="deleteChannelId" class="modal" @click.self="deleteChannelId = null">
        <div class="modal-content">
          <h2>Удалить канал?</h2>
          <p>Вы уверены, что хотите удалить этот канал? Это действие нельзя отменить.</p>
          <div class="modal-actions">
            <button @click="deleteChannel" class="btn btn-danger">Удалить</button>
            <button @click="deleteChannelId = null" class="btn btn-secondary">Отмена</button>
          </div>
        </div>
      </div>

      <!-- Модальное окно подтверждения удаления диалога -->
      <div v-if="deleteDialogId" class="modal" @click.self="deleteDialogId = null">
        <div class="modal-content">
          <h2>Удалить диалог?</h2>
          <p>Вы уверены, что хотите удалить этот диалог? Это действие нельзя отменить.</p>
          <div class="modal-actions">
            <button @click="deleteDialog" class="btn btn-danger">Удалить</button>
            <button @click="deleteDialogId = null" class="btn btn-secondary">Отмена</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()
const channels = ref([])
const dialogs = ref([])
const showHelpModal = ref(false)
const isLoading = ref(false)
const openMenuId = ref(null)
const deleteChannelId = ref(null)
const deleteDialogId = ref(null)

const pendingChannels = computed(() => {
  return channels.value.filter(c => c.bot && !c.botLinked)
})

const confirmedChannels = computed(() => {
  return channels.value.filter(c => c.botLinked)
})

const loadChannels = async () => {
  try {
    channels.value = await authStore.apiFetch('/api/channels')
  } catch (error) {
    console.error('Failed to load channels:', error)
  }
}

const loadDialogs = async () => {
  try {
    dialogs.value = await authStore.apiFetch('/api/dialogs')
  } catch (error) {
    console.error('Failed to load dialogs:', error)
  }
}

const createDialog = async (channelId) => {
  try {
    isLoading.value = true
    const dialog = await authStore.apiFetch('/api/dialogs', {
      method: 'POST',
      body: JSON.stringify({
        channelId: channelId,
        title: null,
        systemPrompt: null
      })
    })
    await loadDialogs()
    // Переход к созданному диалогу
    router.push(`/dialog/${dialog.id}`)
  } catch (error) {
    console.error('Failed to create dialog:', error)
    alert('Не удалось создать диалог: ' + (error.message || 'Неизвестная ошибка'))
  } finally {
    isLoading.value = false
  }
}

const gotoPosts = (channelId) => {
  router.push({ name: 'Posts', params: { channelId: channelId } })
}

const getChannelName = (channelId) => {
  const channel = channels.value.find(c => c.id === channelId)
  return channel?.title || 'Неизвестный канал'
}

const getLastMessage = (dialog) => {
  if (!dialog.messages || dialog.messages.length === 0) {
    return 'Нет сообщений'
  }
  const lastMessage = dialog.messages[dialog.messages.length - 1]
  const content = lastMessage.content || ''
  return content.length > 100 ? content.substring(0, 100) + '...' : content
}


const confirmChannel = async (channelId) => {
  try {
    await authStore.apiFetch(`/api/channels/${channelId}/confirm`, {
      method: 'POST'
    })
    await loadChannels()
  } catch (error) {
    console.error('Failed to confirm channel:', error)
  }
}

const openChannelMenu = (channelId) => {
  openMenuId.value = openMenuId.value === channelId ? null : channelId
}

const openDialogMenu = (dialogId) => {
  openMenuId.value = openMenuId.value === dialogId ? null : dialogId
}

const showDeleteChannelConfirm = (channelId) => {
  openMenuId.value = null
  deleteChannelId.value = channelId
}

const showDeleteDialogConfirm = (dialogId) => {
  openMenuId.value = null
  deleteDialogId.value = dialogId
}

const deleteChannel = async () => {
  if (!deleteChannelId.value) return
  
  try {
    await authStore.apiFetch(`/api/channels/${deleteChannelId.value}`, {
      method: 'DELETE'
    })
    await loadChannels()
    deleteChannelId.value = null
  } catch (error) {
    console.error('Failed to delete channel:', error)
    alert('Не удалось удалить канал: ' + (error.message || 'Неизвестная ошибка'))
  }
}

const deleteDialog = async () => {
  if (!deleteDialogId.value) return
  
  try {
    await authStore.apiFetch(`/api/dialogs/${deleteDialogId.value}`, {
      method: 'DELETE'
    })
    await loadDialogs()
    deleteDialogId.value = null
  } catch (error) {
    console.error('Failed to delete dialog:', error)
    alert('Не удалось удалить диалог: ' + (error.message || 'Неизвестная ошибка'))
  }
}

// Закрываем меню при клике вне его
const handleClickOutside = (event) => {
  // Проверяем, был ли клик на кнопке меню или внутри выпадающего меню
  const target = event.target
  const isMenuButton = target.closest('.menu-button')
  const isMenuDropdown = target.closest('.menu-dropdown')
  // Если клик был на кнопке меню или внутри меню, не закрываем
  if (isMenuButton || isMenuDropdown) {
    return
  }
  
  openMenuId.value = null
}

onMounted(() => {
  loadChannels()
  loadDialogs()
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
/* Все стили перенесены в style.css */
</style>

