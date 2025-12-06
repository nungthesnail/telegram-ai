<template>
    <p class="error">{{ error }}</p>
</template>

<script setup>

const { error } = defineProps({
    error: String
})

</script>
