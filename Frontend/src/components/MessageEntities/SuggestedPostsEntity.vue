<template>
    <div v-if="suggestedPosts.length > 0" class="suggested-posts">
        <h6>Предложенные посты:</h6>
        <ul v-for="post in suggestedPosts" :key="post.id">
            <li :class="post.status === 4 ? ['new-post'] : ['']">
                <b>{{ post.title }}</b>
                <button class="btn btn-primary btn-small" @click.stop="showEditPostModal(post)">
                    Посмотреть
                </button>
            </li>
        </ul>
    </div>

    <div v-if="showEditPopup" class="modal">
        <PostEditModal :post="editPost" :channel-id="channelId" :close-callback="onEditPostModalClose" :save-callback="onPostEditSave">
        </PostEditModal>
    </div>
</template>

<script setup>
import { ref } from 'vue'
import PostEditModal from '../PostEditModal.vue';

const { suggestedPosts, channelId } = defineProps({
    suggestedPosts: Array,
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

</script>

<style scoped>

.btn {
  padding: 0.1rem 0.1rem;
  font-weight: lighter;
  float: right;
  margin-left: 0.5rem;
}

</style>
