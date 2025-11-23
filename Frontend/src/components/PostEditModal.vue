<script setup>

import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const { post, channelId } = defineProps({
    post: Object,
    channelId: String
})

const authStore = useAuthStore()

const postId = post?.id;
const content = ref(post?.content ?? '')
const title = ref(post?.title ?? content.value?.split(new RegExp('(\\n|\\.)'))[0] ?? '')
const scheduledAtUtc = ref(post?.scheduledAtUtc ?? '')
const scheduleChecked = ref(postId !== null)
const errorMessage = ref('')

const handleSave = async () => {
  try {
    const scheduleAtUtc = scheduleChecked.value ? scheduledAtUtc.value : null
    let endpoint;
    let options = {
      body: JSON.stringify({
        'channelId': channelId,
        'title': title.value !== '' ? title.value : null,
        'content': content.value,
        'scheduledAtUtc': scheduleAtUtc
      })
    }

    if (postId) {
      endpoint = `/api/posts/${postId}`
      options.method = 'PUT'
    } else {
      endpoint = '/api/posts'
      options.method = 'POST'
    }

    await authStore.apiFetch(endpoint, options);
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
          <label>Запланировать</label>
          <input type="checkbox" id="doSchedule" v-model="scheduleChecked" />
        </div>
        <div v-if="scheduleChecked" class="form-group">
          <label>Время публикации</label>
          <input v-model="scheduledAtUtc" type="datetime" />
        </div>
        <button type="submit" class="btn btn-primary">Сохранить</button>
        <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
      </form>
    </div>
  </div>
</template>

<style scoped>

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 0.5rem;
  max-width: 500px;
  width: 90%;
}

.modal-content h2 {
  margin-bottom: 1rem;
}

.modal-content p {
  margin-bottom: 0.5rem;
}

.edit-content {
  margin: 1.5rem 0;
}

.edit-content code {
  background: #f5f5f5;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-family: monospace;
  font-size: 0.9em;
}

.modal-content .btn {
  margin-top: 1rem;
  margin-right: 0.5rem;
}

.error {
color: #e74c3c;
margin-top: 1rem;
text-align: center;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #666;
  font-weight: 500;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 0.25rem;
  font-size: 1rem;
}

.form-group textarea {
  width: 100%;
}

</style>
