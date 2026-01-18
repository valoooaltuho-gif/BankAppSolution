# 🏦 BankApp Solution (2025)

![.NET 10](img.shields.io) ![EF Core](img.shields.io) ![License](img.shields.io)

Современная банковская система, построенная на базе **.NET 10** с использованием принципов чистой архитектуры (Clean Architecture). Проект демонстрирует реализацию защищенного REST API с авторизацией и сложной бизнес-логикой управления финансами.

---

## 🚀 Основные возможности

*   **Гибкая система счетов:**
*   
    *   `CheckingAccount`: С поддержкой овердрафта (до 500).
    *   `SavingsAccount`: С запретом на отрицательный баланс и начислением процентов (2%).
    *   `BusinessAccount`: С расширенным лимитом овердрафта (5000) и ежемесячной комиссией.
*   **Безопасность (Security):** 
    *   Аутентификация и авторизация на базе **ASP.NET Core Identity**.
    *   Защита эндпоинтов с помощью **JWT Bearer Tokens**.
*   **Транзакции:** Полное логирование операций (депозиты, снятия, переводы) с историей баланса.
*   **API:** Интерактивная документация через **Swagger UI**.
*   **Тестирование:** Покрытие логики юнит-тестами (**xUnit**, **Moq**, **FluentAssertions**).

---

## 🛠 Технологический стек

*   **Backend:** .NET 10.0 Web API
*   **Database:** SQLite + Entity Framework Core 10
*   **Mapping:** AutoMapper 12
*   **Testing:** xUnit, Moq, FluentAssertions
*   **Middleware:** Глобальная обработка исключений (ErrorHandlingMiddleware)

---

## 🏗 Структура проекта

Решение разделено на независимые слои:
*   **BankApp.Core:** Доменные модели, интерфейсы и кастомные исключения.
*   **BankApp.Services:** Реализация бизнес-логики (`AccountService`).
*   **BankApp.Infrastructure:** Слой данных (EF Core, `BankContext`, репозитории и Identity).
*   **BankApp.Api:** Контроллеры, DTO, конфигурация JWT и Swagger.
*   **BankApp.Tests:** Набор тестов для проверки доменной логики и сервисов.

---

## 🚦 Быстрый старт

### 1. Требования
*   [.NET 10 SDK](dotnet.microsoft.com)

### 2. Установка и запуск
```bash
# Клонирование репозитория
git clone github.com

# Переход в папку решения
cd BankAppSolution

# Восстановление зависимостей
dotnet restore

# Запуск API (база данных SQLite создастся автоматически)
cd BankApp.Api
dotnet run

### 3. Тестирование
bash
# Выполнение всех тестов из корневой папки
dotnet test

🔐 Доступ к API
Для тестирования в Swagger используйте предустановленного пользователя (создается при первом запуске):
Username: admin
Password: Password123!
После авторизации через /api/Auth/login, скопируйте токен и вставьте его в поле Authorize (кнопка вверху страницы Swagger) в формате: Bearer <ваш_токен>.
