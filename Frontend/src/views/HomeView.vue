<template>
  <div class="home">
    <header class="hero">
      <h1>Telegram AI Assistant</h1>
      <p class="banner-lbl">ИИ-ассистент для помощи в ведении Telegram-канала</p>
      <div class="actions">
        <router-link to="/register" class="btn btn-primary">Начать</router-link>
        <router-link to="/login" class="btn btn-secondary">Войти</router-link>
      </div>
    </header>
    <section class="features">
      <h2>Возможности</h2>
      <div class="feature-grid">
        <div class="feature">
          <h3>Управление каналами</h3>
          <p>Легко управляйте несколькими Telegram-каналами из одного интерфейса</p>
        </div>
        <div class="feature">
          <h3>ИИ-ассистент</h3>
          <p>Получайте помощь в создании контента, планировании постов и управлении каналом</p>
        </div>
        <div class="feature">
          <h3>Планирование</h3>
          <p>Планируйте и публикуйте посты автоматически</p>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
</script>

<style scoped>
/* Все стили перенесены в style.css */
</style>

