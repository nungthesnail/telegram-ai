<template>
    <div :class="['message', 'sender-' + message.sender]">
        <div class="message-meta">
            <strong>{{ message.sender === 1 ? 'Вы' : 'Ассистент' }}</strong>
            <small>{{ formatDate(message.createdAtUtc) }}</small>
        </div>
        <div class="message-content">
            <p>{{ message.content }}</p>
            <div v-if="message.suggestedPosts && message.suggestedPosts.length > 0" class="suggested-posts">
                <h6>Предложенные посты:</h6>
                <ul v-for="post in message.suggestedPosts" :key="post.id">
                    <li :class="post.status === 4 ? ['new-post'] : ['']">
                        <b>{{ post.title }}</b><button class="btn btn-primary btn-small" @click.stop="showEditPostModal(post)">Посмотреть</button>
                    </li>
                </ul>
            </div>
        </div>
    </div>

    <div v-if="showEditPopup" class="modal">
        <PostEditModal :post="editPost" :channel-id="channelId" :close-callback="onEditPostModalClose" :save-callback="onPostEditSave">
        </PostEditModal>
    </div>
</template>

<script setup>

import { ref } from 'vue'
import PostEditModal from './PostEditModal.vue';

const { message, channelId } = defineProps({
    message: Object,
    channelId: String
})

const editPost = ref(null)
const showEditPopup = ref(false)

const showEditPostModal = (post) => {
    editPost.value = post
    showEditPopup.value = true
}

const onEditPostModalClose = () => {
    showEditPopup.value = false
    editPost.value = null
}

const onPostEditSave = () => {
    editPost.value.status = 0
    editPost.value = null
    showEditPopup.value = false
}

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('ru-RU')
}

</script>

<style scoped>
.btn {
  padding: 0.1rem 0.1rem;
  font-weight: lighter;
  float: right;
  margin-left: 0.5rem;
}
</style>
