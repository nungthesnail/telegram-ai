<script setup>
  import { ref, onMounted, onUnmounted, computed } from 'vue'
  import { useRoute } from 'vue-router'
  import { useAuthStore } from '../stores/auth'
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
    return posts.value.filter(x => x.status === 0)
  })

  const publishedPosts = computed(() => {
    return posts.value.filter(x => x.status !== 0)
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

      <div v-if="draftsPosts.length > 0" class="posts-section">
        <h2>Черновики</h2>
        <div class="posts-grid">
          <div v-for="post in draftsPosts" :key="post.id" class="post-card">
            <div class="card-header">
              <div class="card-content">
                <h3>{{ post.title || post.content.split(new RegExp('(\\n|\\.)'))[0] }}</h3>
                <p>{{ post.content.slice(null, 20) + (post.content.length > 20 ? '...' : '')  }}</p>
                <p v-if="post.scheduledAtUtc" class="text-secondary">Запланирован на: {{ post.scheduledAtUtc }} (UTC)</p>
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
                <p v-if="post.scheduledAtUtc" class="text-secondary">Запланирован на: {{ post.scheduledAtUtc }} (UTC)</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-if="showEditPopup" class="modal">
        <PostEditModal :post="editPost" :channel-id="channelId"></PostEditModal>
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

.modal-content {
  background: white;
  padding: 2rem;
  border-radius: 0.5rem;
  max-width: 500px;
  width: 90%;
}

.posts-page {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.posts-section {
  margin-top: 3rem;
}

.posts-section h2 {
  margin-bottom: 1.5rem;
  color: #333;
}

.posts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.post-card {
  background: white;
  padding: 1.5rem;
  border-radius: 0.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  position: relative;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.card-content {
  flex: 1;
}

.card-menu {
  position: relative;
}

.channel-card.pending {
  border: 2px solid #f39c12;
}

.channel-card h3 {
  margin-bottom: 0.5rem;
  color: #333;
}

.channel-card p {
  color: #666;
  margin-bottom: 1rem;
}

.channel-actions {
  margin-top: 1rem;
}

.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 0.25rem;
  cursor: pointer;
  text-decoration: none;
  display: inline-block;
  transition: background 0.2s;
  margin: 0.5rem;
}

.btn-primary {
  background: #667eea;
  color: white;
}

.btn-primary:hover {
  background: #5568d3;
}

.btn-secondary {
  background: #95a5a6;
  color: white;
}

.modal {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.dialogs-section {
  margin-top: 3rem;
}

.dialogs-section h2 {
  margin-bottom: 1.5rem;
  color: #333;
}

.dialogs-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1.5rem;
}

.dialog-card {
  background: white;
  padding: 1.5rem;
  border-radius: 0.5rem;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  position: relative;
}

.dialog-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.dialog-info {
  flex: 1;
}

.dialog-info h4 {
  margin-bottom: 0.5rem;
  color: #333;
}

.dialog-channel {
  font-size: 0.875rem;
  color: #667eea;
  margin-bottom: 0.5rem;
}

.dialog-preview {
  font-size: 0.875rem;
  color: #666;
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  line-clamp: 2;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.dialog-card .btn {
  flex-shrink: 0;
}

.menu-button {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #666;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  transition: background 0.2s;
  line-height: 1;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.menu-button:hover {
  background: #f5f5f5;
}

.menu-button.active {
  background: #e0e0e0;
}

.menu-dropdown {
  position: absolute;
  top: 100%;
  right: 0;
  background: white;
  border-radius: 0.25rem;
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  z-index: 100;
  min-width: 120px;
  margin-top: 0.25rem;
}

.menu-item {
  display: block;
  width: 100%;
  padding: 0.75rem 1rem;
  border: none;
  background: none;
  text-align: left;
  cursor: pointer;
  color: #333;
  transition: background 0.2s;
  font-size: 0.875rem;
}

.menu-item:hover {
  background: #f5f5f5;
}

.menu-item.delete {
  color: #e74c3c;
}

.menu-item.delete:hover {
  background: #fee;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
  justify-content: flex-end;
}

.btn-danger {
  background: #e74c3c;
  color: white;
}

.btn-danger:hover {
  background: #c0392b;
}
</style>

