<script setup>
  import { ref, onMounted, onUnmounted, computed } from 'vue'
  import { useRoute } from 'vue-router'
  import { useAuthStore } from '../stores/auth'
  import dayjs from 'dayjs';
  import PostEditModal from '../components/PostEditModal.vue'

  const route = useRoute()
  const channelId = route.params.channelId
  const authStore = useAuthStore()
  const showEditPopup = ref(false)
  const editPost = ref(null)
  const openMenuId = ref(null)
  const deletePostId = ref(null)
  const channel = ref(null)
  const posts = ref([])

  const draftsPosts = computed(() => {
    return posts.value.filter(x => x.status === 0).sort((a, b) => new Date(a.createdAtUtc) > new Date(b.createdAtUtc)
      ? 1
      : a.createdAtUtc == b.createdAtUtc ? 0 : -1)
  })

  const publishedPosts = computed(() => {
    return posts.value.filter(x => x.status !== 0).sort((a, b) => new Date(a.publishedAtUtc) > new Date(b.publishedAtUtc)
      ? 1
      : a.publishedAtUtc == b.publishedAtUtc ? 0 : -1)
  })

  const loadChannel = async () => {
    try {
      channel.value = await authStore.apiFetch(`/api/channels/${channelId}`)
    }
    catch (err) {
      alert('Не удалось загрузить данные канала')
      console.error('Failed to load channel:', err)
    }
  }

  const loadPosts = async () => {
    try {
      posts.value = await authStore.apiFetch(`/api/channels/${channelId}/posts`)
    }
    catch (err) {
      alert('Не удалось загрузить посты')
      console.log('Failed to load posts:', err)
    }
  }

  const openPostMenu = (postId) => {
    openMenuId.value = openMenuId.value === postId ? null : postId
  }

  const showDeletePostConfirm = (postId) => {
    openMenuId.value = null
    deletePostId.value = postId
  }

  const deletePost = async () => {
    if (!deletePostId.value) return
    
    try {
      await authStore.apiFetch(`/api/posts/${deletePostId.value}`, {
        method: 'DELETE'
      })
      await loadPosts()
      deletePostId.value = null
    } catch (error) {
      console.error('Failed to delete post:', error)
      alert('Не удалось отменить пост: ' + (error.message || 'Неизвестная ошибка'))
    }
  }

  const openEditPostPopup = (postId, shouldExists = true) => {
    editPost.value = draftsPosts.value.find(x => x.id === postId)
    editPost.value = editPost.value === undefined ? null : editPost.value
    
    if (!editPost && shouldExists) {
      alert('Пост не найден');
      console.error(`Post id=${postId} isn't found`);
      return;
    }

    showEditPopup.value = true;
  }

  const confirmPost = async (postId) => {
    try {
      await authStore.apiFetch(`/api/posts/${postId}/publish`, {
        method: 'POST'
      })
      await loadPosts()
    } catch (error) {
      console.error('Failed to publish post:', error)
      alert('Не удалось опубликовать пост: ' + (error.message || 'Неизвестная ошибка'))
    }
  }

  const handleClickOutside = (event) => {
    const target = event.target
    const isMenuButton = target.closest('.menu-button')
    const isMenuDropdown = target.closest('.menu-dropdown')
    
    if (isMenuButton || isMenuDropdown) {
      return
    }
    
    openMenuId.value = null
  }

  const onPostEditClose = () => {
    showEditPopup.value = false
    editPost.value = null
  }

  const onPostEditSave = async () => {
    onPostEditClose()
    await loadChannel()
    await loadPosts()
  }

  onMounted(() => {
    loadChannel()
    loadPosts()
    document.addEventListener('click', handleClickOutside)
  })

  onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside)
  })
</script>

<template>
  <div class="posts-page">
    <div class="container">
      <div class="header">
        <router-link to="/channels" class="btn-back">← Назад к каналам</router-link>
        <h1>Посты</h1>
        <button @click="openEditPostPopup(null, false)" class="btn btn-primary">Новый пост</button>
      </div>

      <i v-if="channel">Канал: {{ channel.title }}</i>

      <div v-if="draftsPosts.length > 0" class="posts-section">
        <h2>Черновики</h2>
        <div class="posts-grid">
          <div v-for="post in draftsPosts" :key="post.id" class="post-card">
            <div class="card-header">
              <div class="card-content">
                <h3>{{ post.title || post.content.split(new RegExp('(\\n|\\.)'))[0] }}</h3>
                <p>{{ post.content.slice(null, 20) + (post.content.length > 20 ? '...' : '')  }}</p>
                <p v-if="post.scheduledAtUtc" class="text-secondary">Запланирован на: <br><b>{{ dayjs(post.scheduledAtUtc).format("YYYY.MM.DD HH:mm:ss") }}</b></p>
              </div>
              <div class="card-menu">
                <button @click.stop="openPostMenu(post.id)" class="menu-button" :class="{ active: openMenuId === post.id }">
                  ⋮
                </button>
                <div v-if="openMenuId === post.id" class="menu-dropdown" @click.stop>
                  <button @click="showDeletePostConfirm(post.id)" class="menu-item delete">Отменить</button>
                </div>
              </div>
            </div>
            <button @click="openEditPostPopup(post.id)" class="btn btn-secondary">Редактировать</button>
            <button @click="confirmPost(post.id)" class="btn btn-primary">Опубликовать</button>
          </div>
        </div>
      </div>
      
      <div v-if="publishedPosts.length > 0" class="posts-section">
        <h2>Опубликованные посты</h2>
        <div class="posts-grid">
          <div v-for="post in publishedPosts" :key="post.id" class="post-card">
            <div class="card-header">
              <div class="card-content">
                <h3>{{ post.title || post.content.split(new RegExp('(\\n|\\.)'))[0] }}</h3>
                <p>{{ post.content.slice(null, 20) + (post.content.length > 20 ? '...' : '')  }}</p>
                <p v-if="post.scheduledAtUtc" class="text-secondary">Запланирован на: <br><b>{{ dayjs(post.scheduledAtUtc).format("YYYY.MM.DD HH:mm:ss") }}</b></p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-if="showEditPopup" class="modal">
        <PostEditModal :post="editPost" :channel-id="channelId" :close-callback="onPostEditClose" :save-callback="onPostEditSave"></PostEditModal>
      </div>

      <!-- Модальное окно подтверждения удаления поста -->
      <div v-if="deletePostId" class="modal" @click.self="deleteDialogId = null">
        <div class="modal-content">
          <h2>Отменить пост?</h2>
          <p>Вы уверены, что хотите отменить этот пост? Черновик будет удален. Это действие нельзя отменить.</p>
          <div class="modal-actions">
            <button @click="deletePost" class="btn btn-danger">Удалить</button>
            <button @click="deletePostId = null" class="btn btn-secondary">Отмена</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Все стили перенесены в style.css */
</style>

