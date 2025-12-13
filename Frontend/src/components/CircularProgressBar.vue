<template>
  <div class="progress-container">
    <svg class="progress-ring" :width="size" :height="size">
      <circle
        class="progress-ring__circle-bg"
        :stroke="bgColor"
        :stroke-width="strokeWidth"
        fill="transparent"
        :r="radius"
        :cx="center"
        :cy="center"
      />
      <circle
        class="progress-ring__circle"
        :stroke="color"
        :stroke-width="strokeWidth"
        fill="transparent"
        :r="radius"
        :cx="center"
        :cy="center"
        :style="circleStyle"
      />
    </svg>
    <div class="progress-text">
      {{ percentage.toFixed(0) }}%
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  percentage: {
    type: Number,
    required: true,
    validator: value => value >= 0 && value <= 100
  },
  size: {
    type: Number,
    default: 100
  },
  strokeWidth: {
    type: Number,
    default: 10
  },
  color: {
    type: String,
    default: '#667eea'
  },
  bgColor: {
    type: String,
    default: '#e4e7ec'
  }
})

const radius = computed(() => (props.size - props.strokeWidth) / 2)
const center = computed(() => props.size / 2)
const circumference = computed(() => 2 * Math.PI * radius.value)
const offset = computed(() => circumference.value - (props.percentage / 100) * circumference.value)
const circleStyle = computed(() => ({
  strokeDasharray: `${circumference.value} ${circumference.value}`,
  strokeDashoffset: offset.value
}))
</script>

<style scoped>
.progress-container {
  position: relative;
  display: inline-block;
}

.progress-ring {
  transform-origin: center;
  display: block;
}

.progress-ring__circle-bg {
  opacity: 0.3;
}

.progress-ring__circle {
  transition: stroke-dashoffset 0.35s;
  transform: rotate(-90deg);
  transform-origin: center;
}

.progress-text {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 1.2em;
  font-weight: bold;
  color: var(--color-text);
}
</style>

