# Docker Setup для Telegram AI Assistant

## Структура

- **Frontend** - Vue.js приложение, собирается в статические файлы и обслуживается через nginx
- **Backend** - ASP.NET Core API, работает на порту 8080
- **Nginx** - Reverse proxy, который:
  - Перенаправляет `/api/*` запросы на backend
  - Перенаправляет все остальные запросы на frontend
- **PostgreSQL** - База данных на порту 5432

## Запуск

1. Убедитесь, что папка `C:/var/lib/postgresql` существует (или измените путь в docker-compose.yml)

2. Запустите все сервисы:
```bash
docker-compose up -d
```

3. Откройте браузер и перейдите на:
   - `http://localhost` - через nginx (рекомендуется)
   - `http://localhost:3000` - напрямую frontend
   - `http://localhost:8080` - напрямую backend API

## Переменные окружения

Создайте файл `.env` в корне проекта (опционально):
```
TELEGRAM_BOT_TOKEN=your_bot_token_here
```

Или установите переменную окружения перед запуском:
```bash
$env:TELEGRAM_BOT_TOKEN="your_bot_token_here"
docker-compose up -d
```

## Остановка

```bash
docker-compose down
```

Для полной очистки (включая volumes):
```bash
docker-compose down -v
```

## Полезные команды

Просмотр логов:
```bash
docker-compose logs -f [service_name]
```

Пересборка после изменений:
```bash
docker-compose up -d --build
```

Вход в контейнер:
```bash
docker exec -it tgassistant-backend bash
docker exec -it tgassistant-frontend sh
docker exec -it tgassistant-nginx sh
```

## Миграции базы данных

Миграции выполняются автоматически при запуске backend (см. Program.cs).

Для ручного выполнения:
```bash
docker exec -it tgassistant-backend dotnet ef database update
```

