<script setup>
import { ref } from 'vue'
import { useAuthStore } from '../stores/auth'

const { post, channelId, closeCallback, saveCallback } = defineProps({
  post: Object,
  channelId: String,
  closeCallback: Function,
  saveCallback: Function
})

const authStore = useAuthStore()

const postId = post?.id
const content = ref(post?.content ?? '')
const title = ref(post?.title ?? content.value?.split(new RegExp('(\\n|\\.)'))[0] ?? '')
const scheduledAtUtc = ref(post?.scheduledAtUtc ?? '')
const scheduleChecked = ref(postId !== null)
const errorMessage = ref('')

const handleSave = async () => {
  try {
    const scheduleAtUtc = scheduleChecked.value ? scheduledAtUtc.value : null
    let endpoint
    let options = {
      body: JSON.stringify({
        channelId: channelId,
        title: title.value !== '' ? title.value : null,
        content: content.value,
        scheduledAtUtc: scheduleAtUtc
      })
    }

    if (postId) {
      endpoint = `/api/posts/${postId}`
      options.method = 'PUT'
    } else {
      endpoint = '/api/posts'
      options.method = 'POST'
    }

    await authStore.apiFetch(endpoint, options)
    await saveCallback()
  } catch (err) {
    alert('Не удалось сохранить пост.')
    console.error('Failed to save post', err)
  }
}
</script>

<template>
  <div class="modal-content">
    <h2 v-if="post">Редактирование черновика</h2>
    <h2 v-else>Новый черновик</h2>
    <div class="edit-content">
      <form @submit.prevent="handleSave">
        <div class="form-group">
          <label>Название</label>
          <input v-model="title" type="text" placeholder="Название черновика..." />
        </div>
        <div class="form-group">
          <label>Текст поста</label>
          <textarea v-model="content" rows="20" required placeholder="Главные новости на сегодня..."></textarea>
        </div>
        <div class="form-group">
          <label><input type="checkbox" id="doSchedule" v-model="scheduleChecked" /> Запланировать</label>
        </div>
        <div v-if="scheduleChecked" class="form-group">
          <label>Время публикации</label>
          <input v-model="scheduledAtUtc" type="datetime-local" required />
        </div>
        <button type="submit" class="btn btn-primary">Сохранить</button>
        <button type="button" class="btn btn-secondary" @click="closeCallback">Отменить</button>
        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
      </form>
    </div>
  </div>
</template>
