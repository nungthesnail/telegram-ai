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
.home {
  min-height: 100vh;
}

.hero {
  text-align: center;
  padding: 4rem 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.hero h1 {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.hero p {
  font-size: 1.25rem;
  margin-bottom: 2rem;
}

.actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.btn {
  padding: 0.75rem 2rem;
  border-radius: 0.5rem;
  text-decoration: none;
  font-weight: 600;
  transition: transform 0.2s;
}

.btn:hover {
  transform: translateY(-2px);
}

.btn-primary {
  background: white;
  color: #667eea;
}

.btn-secondary {
  background: transparent;
  color: white;
  border: 2px solid white;
}

.features {
  padding: 4rem 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.features h2 {
  text-align: center;
  font-size: 2rem;
  margin-bottom: 3rem;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
}

.feature {
  padding: 2rem;
  border-radius: 0.5rem;
  background: #f5f5f5;
}

.feature h3 {
  margin-bottom: 1rem;
  color: #667eea;
}
</style>

