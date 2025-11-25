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
                        <b>{{ post.title }}</b><button class="btn" @click.stop="showEditPostModal(post)">Посмотреть</button>
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
  font-size: 0.7rem;
  opacity: 0.7;
  white-space: nowrap;
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

.btn {
  padding: 0.1rem 0.1rem;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 0.25rem;
  cursor: pointer;
  font-weight: lighter;
  float: right;
  margin-left: 0.5rem;
}

.new-post {
    color: green;
}

.suggested-posts {
    margin-top: 1.5rem;
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

ul li {
  list-style-type: square;
  list-style-position: inside;
  text-wrap: stable;
  text-wrap-mode: wrap;
  overflow-wrap: anywhere;
  vertical-align: middle;
  line-height: normal;
  margin-top: 0.5rem;
}

</style>
