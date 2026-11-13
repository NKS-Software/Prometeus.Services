# Prometeus.Services

[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13-blue)](https://learn.microsoft.com/dotnet/csharp/)
[![Tests](https://img.shields.io/badge/tests-80%2B-green)](https://github.com/NKS-Software/Prometeus.Services.Tests)

Сервисный слой автоматизированной банковской системы (АБС) на C#. Содержит бизнес-логику поверх ядра [`Prometeus.Core`](https://github.com/NKS-Software/Prometeus.Core).

### 🧪 Тесты
- **80 тестов** (xUnit) на сервисный слой.
- Покрытие всех статусов, негативных сценариев и граничных случаев.
- Тесты — в [отдельном репозитории](https://github.com/NKS-Software/Prometeus.Services.Tests).

---

## 🚀 Возможности

### 🏦 Клиенты (`ClientService`)
- Создание, обновление и удаление клиентов.
- Смена email и номера телефона с проверкой на уникальность.
- Каскадная загрузка счетов и карт клиента.

### 💳 Счета (`AccountService`)
- Открытие и закрытие счетов.
- Получение реквизитов счёта (включая корреспондентский счёт).
- Проверка принадлежности счёта клиенту.

### 💳 Карты (`CardService`)
- Выпуск физических и виртуальных карт.
- Заморозка, разморозка и перманентная блокировка.
- Смена PIN-кода.
- Получение полной информации о карте (включая CVC).

### 💸 Транзакции (`TransactionService`)
- Переводы между счетами и на карты внутри банка.
- **Двойная запись** (дебет = кредит).
- Атомарность (транзакции БД с откатом при ошибке).
- Проверка достаточности средств, валюты и статуса карты.

---

## 📦 Стек

| Компонент | Версия |
|-----------|--------|
| .NET | 10.0 |
| C# | 13 |
| Зависимости | `Prometeus.Core` |

---

## 🏗️ Архитектура

├── Configs/ # BankDetails (БИК, лицензия, BIN'ы, коды валют)</br>
├── DataAnnotations/  # Кастомные атрибуты (Name, Surname, CardNumber, ...) </br>
├── Models/ </br>
│├── DTOs/ # ClientDTO, AccountDTO, CardDTO, TransactionDTO </br>
│├── Requests/ # AddClientRequest, SendToAccountRequest, ... </br>
│└── Responses/ # AddClientResponse
├── AccountService.cs # Сервис управления счетами </br>
├── CardService.cs # Сервис управления картами </br>
├── ClientService.cs # Сервис управления клиентами </br>
└── TransactionService.cs # Сервис управления переводами </br>

## 🛠️ Пример использования

### Инициализация сервисов

```csharp
var context = new DatabaseContext.Builder()
    .UseFile("prometeus.db")
    // ... регистрация репозиториев ...
    .Build();

var bankDetails = new BankDetails
{
    BIK = "045655635",
    Name = "Prometeus Bank",
    BINs = new() { ["Mir"] = ["220204"], ["Visa"] = ["440000"] },
    CurrencyCodes = new() { ["RUB"] = 643, ["USD"] = 840 }
};

var clientService = new ClientService(context);
var accountService = new AccountService(context, bankDetails);
var cardService = new CardService(context, bankDetails, keyStore);
var transactionService = new TransactionService(context, bankDetails);
```

### Создание клиента
```csharp
var request = new AddClientRequest
{
    Name = "Иван",
    Surname = "Иванов",
    BirthDate = new DateOnly(2000, 1, 1),
    Email = "ivan@example.com",
    Phone = "+79061571234"
};

var response = await clientService.AddAsync(request);
```

### Перевод между счетами
```csharp
var request = new SendToAccountRequest
{
    FromAccount = "40802643000000000001",
    ToAccount = "40802643000000000002",
    Amount = 500
};

var response = await transactionService.SendToAccountAsync(clientId, request);
```

## 🧠 Принципы

| Принцип | Как реализован |
|-----------|--------|
| Безопасность | Проверка прав доступа к счетам и картам |
| Атомарность | Транзакции БД с Rollback при ошибке |
| Консистентность | Двойная запись (сумма дебетов = сумме кредитов) |
| Прагматизм | Без лишних интерфейсов; DI через конструктор |

## 📄 Лицензия
Этот код предоставлен только для ознакомления. Любое использование без письменного разрешения автора запрещено.
