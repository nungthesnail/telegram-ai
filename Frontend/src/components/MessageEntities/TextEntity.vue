<template>
    <p>{{ text }}</p>
</template>

<script setup>

const { text } = defineProps({
    text: String
})

</script>
