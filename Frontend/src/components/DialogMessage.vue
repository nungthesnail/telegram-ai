<template>
    <div :class="['message', 'sender-' + message.sender]">
        <div class="message-meta">
            <strong>{{ message.sender === 1 ? 'Вы' : 'Ассистент' }}</strong>
            <small>{{ formatDate(message.createdAtUtc) }}</small>
        </div>

        <div class="message-content">
            <div v-for="entity in entities" class="message-entity">
                <TextMessageEntity v-if="entity['$type'] === 'text'" :text="entity.text"></TextMessageEntity>

                <SuggestedPostsEntity v-if="entity['$type'] === 'suggestedPosts'" :suggested-posts="entity.posts" :channel-id="channelId">
                </SuggestedPostsEntity>

                <ErrorEntity v-if="entity['$type'] === 'error'" :error="entity.error"></ErrorEntity>
            </div>
        </div>
    </div>
</template>

<script setup>

import { computed } from 'vue'
import TextMessageEntity from './MessageEntities/TextEntity.vue'
import SuggestedPostsEntity from './MessageEntities/SuggestedPostsEntity.vue';
import ErrorEntity from './MessageEntities/ErrorEntity.vue';

const { message, channelId } = defineProps({
    message: Object,
    channelId: String
})

const entities = computed(() => message.entities)

const formatDate = (date) => {
  if (!date) return ''
  return new Date(date).toLocaleString('ru-RU')
}

</script>
