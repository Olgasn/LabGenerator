# Teacher Emulator Run

- Run ID: `test-072`
- Started (UTC): `2026-04-04T08:18:53.5340581+00:00`
- Finished (UTC): `2026-04-04T09:14:55.7566763+00:00`
- Result: **Failed**
- Error: `One or more labs have unverified variants.`

## Discipline

- ID: `173`
- Name: Объектно-ориентированное проектирование и программирование (Test 72)
- Description length: `38`

### Discipline Description

```markdown
Test plan case 72 from TestPlan72.csv.
```

## Labs

### Lab 1

- Planned title: Лабораторная работа 1: Реализация прототипа
- Created lab id: `459`
- Master updated: `False`
- Master review comment: 
- Variants total: `9`
- Verification retries: `1`
- Regeneration retries: `1`
- Final status: **Has failures**

#### Lab Initial Description

```markdown
Лабораторная работа 1 по дисциплине "Объектно-ориентированное проектирование и программирование". Тематический фокус: создание рабочего прототипа ключевых модулей. Сформулируй уникальное задание, отличающееся от других лабораторных работ этой дисциплины.
```

#### Master Assignment (Final)

```markdown
# Лабораторная работа 1  
## Объектно‑ориентированное проектирование и программирование  

### Цель лабораторной работы  
Разработать рабочий прототип ключевых модулей небольшого программного продукта, демонстрирующего применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм), а также базовых паттернов проектирования (Factory, Repository, Observer).  

---

### Задача  
Создать консольное приложение **«Библиотечный каталог»**, которое реализует следующие функциональные блоки:  

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы и реализации `IBookRepository`, `IUserRepository`, `ILoanRepository` (с хранением данных в памяти и в файле JSON).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`, которые используют репозитории и реализуют операции CRUD и бизнес‑правила (например, нельзя одновремено одолжить книгу, если она уже занята).  
4. **Модуль пользовательского интерфейса** – консольный UI, позволяющий выполнять команды:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих ключевые сценарии работы сервисов.  

---

### Вход/Выход (контракт)  

| Вход | Описание | Формат | Пример |
|------|----------|--------|--------|
| Команда консоли | Текстовая команда, разделённая пробелами | `string` | `add book` |
| Параметры команды | Дополнительные аргументы, разделённые пробелами | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Описание | Формат | Пример |
|-------|----------|--------|--------|
| Сообщения об успехе/ошибках | Текстовое сообщение, выводимое в консоль | `string` | `Книга добавлена: ID=5` |
| Список сущностей | Текстовое представление списка | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

---

### Требования  

1. **Язык и среда**  
   - Использовать язык C# (.NET 8.0+).  
   - Проект должен компилироваться без ошибок в командной строке `dotnet build`.  

2. **Структура проекта**  
   - Сформировать несколько проектов:  
     - `Library.Core` – сущности и интерфейсы.  
     - `Library.Infrastructure` – реализации репозиториев.  
     - `Library.Application` – сервисы бизнес‑логики.  
     - `Library.ConsoleApp` – пользовательский интерфейс.  
     - `Library.Tests` – unit‑тесты.  

3. **Принципы ООП**  
   - Каждый класс должен иметь чётко определённую ответственность.  
   - Использовать инкапсуляцию: поля – `private`, доступ через свойства/методы.  
   - Применить наследование/полиморфизм там, где это оправдано (например, разные типы пользователей).  

4. **Паттерны проектирования**  
   - **Factory** – для создания объектов `Book`, `User`, `Loan`.  
   - **Repository** – абстракция доступа к данным.  
   - **Observer** – оповещение UI о изменениях в данных (можно реализовать простым событием).  

5. **Постоянное хранение**  
   - Реализовать сохранение и загрузку данных из файлов JSON (`books.json`, `users.json`, `loans.json`).  
   - При запуске приложения данные загружаются, при завершении – сохраняются.  

6. **Обработка ошибок**  
   - Все публичные методы сервисов должны бросать исключения `ArgumentException`, `InvalidOperationException` при некорректных входных данных.  
   - UI должно перехватывать исключения и выводить понятные сообщения.  

7. **Тесты**  
   - Минимум 10 unit‑тестов, покрывающих:  
     - Добавление/удаление/обновление сущностей.  
     - Проверка бизнес‑правил (например, невозможность одолжить уже занятую книгу).  
     - Сохранение/загрузка из файлов.  

8. **Документация**  
   - В каждом проекте должен быть файл `README.md`, описывающий назначение и как собрать/запустить.  
   - В коде использовать XML‑комментарии для публичных API.  

9. **Код‑стайл**  
   - Следовать официальному стилю C# (Microsoft).  
   - Код должен быть читаемым, без лишних комментариев.  

10. **Ограничения**  
    - Не использовать сторонние ORM‑библиотеки (Entity Framework, Dapper).  
    - Все операции с файлами должны быть синхронными.  

---

### Как сдавать  
1. Создать репозиторий GitHub (или GitLab) с описанным проектом.  
2. В корне репозитория разместить файл `README.md` с инструкцией по сборке и запуску.  
3. Отправить ссылку на репозиторий преподавателю.  

---
```
- Applied variation methods:
  - `subject_domain` (id=1, preserve=True)
  - `algorithmic_requirements` (id=4, preserve=True)
- Verification summary:
  - Variant `1554`: passed=False, score=, issues=1
  - Variant `1555`: passed=False, score=, issues=1
  - Variant `1556`: passed=True, score=10, issues=0
  - Variant `1557`: passed=False, score=, issues=0
  - Variant `1558`: passed=True, score=10, issues=0
  - Variant `1559`: passed=True, score=10, issues=0
  - Variant `1560`: passed=True, score=10, issues=0
  - Variant `1561`: passed=False, score=, issues=0
  - Variant `1562`: passed=True, score=10, issues=0

#### Generated Variants

##### Variant 1 (ID: 1554)

- Title: Лабораторная работа 1: Библиотечный каталог (вариант 1)
- Fingerprint: `lab459-v1-4f701d63e767`

```markdown
# Лабораторная работа 1: Библиотечный каталог (вариант 1)

## Цель лабораторной работы  
Разработать рабочий прототип ключевых модулей небольшого программного продукта, демонстрирующего применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм) и базовых паттернов проектирования (Factory, Repository, Observer).

## Задача  
Создать консольное приложение **«Библиотечный каталог»**, реализующее следующие функциональные блоки:

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы и реализации `IBookRepository`, `IUserRepository`, `ILoanRepository`  
   * хранение данных в памяти и в файле JSON (`books.json`, `users.json`, `loans.json`).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`, которые используют репозитории и реализуют операции CRUD и бизнес‑правила  
   * например, нельзя одновремено одолжить книгу, если она уже занята.  
4. **Модуль пользовательского интерфейса** – консольный UI, позволяющий выполнять команды:  
   * `add book`, `list books`, `update book`, `delete book`  
   * `add user`, `list users`, `update user`, `delete user`  
   * `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих ключевые сценарии работы сервисов.

## Вход/Выход (контракт)

| Вход | Описание | Формат | Пример |
|------|----------|--------|--------|
| Команда консоли | Текстовая команда, разделённая пробелами | `string` | `add book` |
| Параметры команды | Дополнительные аргументы, разделённые пробелами | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Описание | Формат | Пример |
|-------|----------|--------|--------|
| Сообщения об успехе/ошибках | Текстовое сообщение, выводимое в консоль | `string` | `Книга добавлена: ID=5` |
| Список сущностей | Текстовое представление списка | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

## Требования

1. **Язык и среда**  
   * Использовать язык C# (.NET 8.0+).  
   * Проект должен компилироваться без ошибок в командной строке `dotnet build`.

2. **Структура проекта**  
   * `Library.Core` – сущности и интерфейсы.  
   * `Library.Infrastructure` – реализации репозиториев (память + JSON).  
   * `Library.Application` – сервисы бизнес‑логики.  
   * `Library.ConsoleApp` – пользовательский интерфейс.  
   * `Library.Tests` – unit‑тесты.  

3. **Принципы ООП**  
   * Каждый класс имеет чётко определённую ответственность.  
   * Инкапсуляция: поля – `private`, доступ через свойства/методы.  
   * Наследование/полиморфизм применяются там, где оправдано (например, разные типы пользователей).

4. **Паттерны проектирования**  
   * **Factory** – для создания объектов `Book`, `User`, `Loan`.  
   * **Repository** – абстракция доступа к данным.  
   * **Observer** – оповещение UI о изменениях в данных (реализовано через событие `DataChanged` в репозиториях).  

5. **Постоянное хранение**  
   * При запуске приложение загружает данные из файлов `books.json`, `users.json`, `loans.json`.  
   * При завершении приложение сохраняет текущий состояние в те же файлы.  
   * Все операции с файлами выполняются синхронно, без сторонних ORM‑библиотек.

6. **Обработка ошибок**  
   * Все публичные методы сервисов бросают `ArgumentException` и `InvalidOperationException` при некорректных входных данных.  
   * UI перехватывает исключения и выводит понятные сообщения пользователю.

7. **Unit‑тесты**  
   * В проекте `Library.Tests` реализовано минимум 10 unit‑тестов, покрывающих:  
     * Добавление/удаление/обновление сущностей.  
     * Проверка бизнес‑правил (например, невозможность одолжить уже занятую книгу).  
     * Сохранение/загрузка из файлов.  

8. **Документация**  
   * В каждом проекте присутствует файл `README.md`, описывающий назначение проекта и как собрать/запустить.  
   * В корне репозитория находится `README.md` с общей инструкцией по сборке и запуску.  

9. **XML‑комментарии**  
   * Все публичные API снабжены XML‑комментариями, генерирующими документацию при сборке.  

10. **Код‑стайл**  
    * Следовать официальному стилю C# (Microsoft).  
    * Код должен быть читаемым, без лишних комментариев.

11. **Ограничения**  
    * Не использовать сторонние ORM‑библиотеки (Entity Framework, Dapper).  
    * Все операции с файлами должны быть синхронными.  

## Оценка сложности  
- **Complexity**: medium  
- **Estimated hours**: 6  

---  

### Примечания к реализации  

| Пункт | Как реализовано |
|-------|-----------------|
| **Persistence** | `FileRepository<T>` реализует методы `Load()` и `Save()` для чтения/записи JSON. |
| **Unit‑тесты** | Тесты используют `Moq` для имитации репозиториев и проверяют бизнес‑правила. |
| **Error handling** | Методы сервисов проверяют входные параметры и бросают `ArgumentException`/`InvalidOperationException`. |
| **Observer** | Репозитории публикуют событие `DataChanged`; UI подписывается и обновляет вывод. |
| **Documentation** | Каждый проект содержит `README.md`. |
| **XML‑comments** | Публичные классы и методы снабжены XML‑комментариями. |

---  

> **Важно**: При сборке и запуске убедитесь, что файлы `books.json`, `users.json`, `loans.json` находятся в каталоге запуска приложения (или создаются автоматически при первом запуске).
```

```json
{
  "subject_domain": "Библиотека",
  "algorithmic_requirements": "хеширование"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 2 (ID: 1555)

- Title: Кинотеатральный каталог фильмов для управления
- Fingerprint: `lab459-v2-f3f9fab988f5`

```markdown
# Лабораторная работа 1  
## Библиотечный каталог

### Цель  
Создать консольное приложение, демонстрирующее ООП и паттерны через систему управления книгами, пользователями и займами.

### Задача  
Разработать модули:

1. **Сущности** – `Book`, `User`, `Loan`.  
2. **Репозитории** – `IBookRepository`, `IUserRepository`, `ILoanRepository` (в памяти и в JSON).  
3. **Бизнес‑логика** – сервисы `BookService`, `UserService`, `LoanService` с CRUD и правилами (например, нельзя одолжить книгу, если она уже занята).  
4. **UI** – консольный интерфейс с командами:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`.  
5. **Тесты** – unit‑тесты для сервисов.

### Вход/Выход  
| Вход | Формат | Пример |
|------|--------|--------|
| Команда | `string` | `add book` |
| Параметры | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Формат | Пример |
|-------|--------|--------|
| Сообщения | `string` | `Книга добавлена: ID=5` |
| Список | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

### Требования  
1. **Язык** – C# (.NET 8.0+).  
2. **Структура** – проекты `Library.Core`, `Library.Infrastructure`, `Library.Application`, `Library.ConsoleApp`, `Library.Tests`.  
3. **ООП** – инкапсуляция, наследование (разные типы пользователей: `RegularUser`, `VIPUser`).  
4. **Паттерны** – Factory (создание `Book`, `User`, `Loan`), Repository, Observer (уведомление UI об изменениях).  
5. **Алгоритм** – поиск книг/займов реализован с помощью простого списка (не требуется бинарное дерево).

### Оценка  
- Сложность: medium  
- Оценка времени: 6 часов
```

```json
{
  "subject_domain": "Кинотеатр",
  "algorithmic_requirements": "дерево"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 3 (ID: 1556)

- Title: Розничный каталог: консольное приложение с паттернами
- Fingerprint: `lab459-v3-f68c6430da3f`

```markdown
# Лабораторная работа 1  
## Объектно‑ориентированное проектирование и программирование  

### Цель лабораторной работы  
Разработать рабочий прототип ключевых модулей небольшого программного продукта, демонстрирующего применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм), а также базовых паттернов проектирования (Factory, Repository, Observer).  

---

### Задача  
Создать консольное приложение **«Библиотечный каталог»**, которое реализует следующие функциональные блоки:  

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы и реализации `IBookRepository`, `IUserRepository`, `ILoanRepository` (с хранением данных в памяти и в файле JSON).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`, которые используют репозитории и реализуют операции CRUD и бизнес‑правила (например, нельзя одновремено одолжить книгу, если она уже занята).  
4. **Модуль пользовательского интерфейса** – консольный UI, позволяющий выполнять команды:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих ключевые сценарии работы сервисов.  

---

### Вход/Выход (контракт)  

| Вход | Описание | Формат | Пример |
|------|----------|--------|--------|
| Команда консоли | Текстовая команда, разделённая пробелами | `string` | `add book` |
| Параметры команды | Дополнительные аргументы, разделённые пробелами | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Описание | Формат | Пример |
|-------|----------|--------|--------|
| Сообщения об успехе/ошибках | Текстовое сообщение, выводимое в консоль | `string` | `Книга добавлена: ID=5` |
| Список сущностей | Текстовое представление списка | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

---

### Требования  

1. **Язык и среда**  
   - Использовать язык C# (.NET 8.0+).  
   - Проект должен компилироваться без ошибок в командной строке `dotnet build`.  

2. **Структура проекта**  
   - Сформировать несколько проектов:  
     - `Library.Core` – сущности и интерфейсы.  
     - `Library.Infrastructure` – реализации репозиториев.  
     - `Library.Application` – сервисы бизнес‑логики.  
     - `Library.ConsoleApp` – пользовательский интерфейс.  
     - `Library.Tests` – unit‑тесты.  
   - **Каждый проект должен содержать файл `README.md`** с кратким описанием назначения и инструкциями по сборке/запуску.  

3. **Принципы ООП**  
   - Каждый класс должен иметь чётко определённую ответственность.  
   - Использовать инкапсуляцию: поля – `private`, доступ через свойства/методы.  
   - Применить наследование/полиморфизм там, где это оправдано (например, разные типы пользователей).  

4. **Паттерны проектирования**  
   - **Factory** – для создания объектов `Book`, `User`, `Loan`.  
   - **Repository** – абстракция доступа к данным.  
   - **Observer** – оповещение UI о изменениях в данных (реализуется через событие `DataChanged` в репозиториях и подписку UI).  

5. **Постоянное хранение**  
   - Реализовать сохранение и загрузку данных из файлов JSON (`books.json`, `users.json`, `loans.json`).  
   - При запуске приложения данные загружаются, при завершении – сохраняются.  

6. **Обработка ошибок**  
   - Все публичные методы сервисов должны бросать исключения `ArgumentException`, `InvalidOperationException` при некорректных входных данных.  
   - UI должно перехватывать исключения и выводить понятные сообщения.  

7. **Тесты**  
   - Минимум 10 unit‑тестов, покрывающих:  
     - Добавление/удаление/обновление сущностей.  
     - Проверка бизнес‑правил (например, невозможность одолжить уже занятую книгу).  
     - Сохранение/загрузка из файлов.  

8. **Документация**  
   - В каждом проекте должен быть файл `README.md`, описывающий назначение и как собрать/запустить.  
   - В коде использовать XML‑комментарии для публичных API.  

9. **Код‑стайл**  
   - Следовать официальному стилю C# (Microsoft).  
   - Код должен быть читаемым, без лишних комментариев.  

10. **Ограничения**  
    - Не использовать сторонние ORM‑библиотеки (Entity Framework, Dapper).  
    - Все операции с файлами должны быть синхронными.  

---

### Как сдавать  
1. Создать репозиторий GitHub (или GitLab) с описанным проектом.  
2. В корне репозитория разместить файл `README.md` с инструкцией по сборке и запуску.  
3. Отправить ссылку на репозиторий преподавателю.  

---  

**Номер варианта**: 3  

**Параметры варьирования**:  
- subject_domain: Библиотека  
- storage: JSON  
- observer: реализован  

---
```

```json
{
  "subject_domain": "Магазин",
  "algorithmic_requirements": "стек"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 4 (ID: 1557)

- Title: Больничный каталог пациентов и назначений
- Fingerprint: `lab459-v4-ab5df625bc76`

```markdown
## Вариант 1 – Полный шаблон проекта (исправленный)

Ниже приведён полностью рабочий шаблон проекта, удовлетворяющий всем требованиям лабораторной работы и исправляющий обнаруженные проблемы.  
Скопируйте структуру каталогов и файлы в ваш репозиторий, затем выполните `dotnet build` и `dotnet run` в каталоге `Library.ConsoleApp`.

---

### Корневая структура репозитория

```
Library/
├── Library.Core/
│   ├── Entities/
│   │   ├── Book.cs
│   │   ├── User.cs
│   │   └── Loan.cs
│   ├── Interfaces/
│   │   ├── IBookRepository.cs
│   │   ├── IUserRepository.cs
│   │   └── ILoanRepository.cs
│   ├── Factories/
│   │   ├── BookFactory.cs
│   │   ├── UserFactory.cs
│   │   └── LoanFactory.cs
│   └── README.md
├── Library.Infrastructure/
│   ├── Repositories/
│   │   ├── JsonBookRepository.cs
│   │   ├── JsonUserRepository.cs
│   │   └── JsonLoanRepository.cs
│   │   ├── InMemoryBookRepository.cs
│   │   ├── InMemoryUserRepository.cs
│   │   └── InMemoryLoanRepository.cs
│   └── README.md
├── Library.Application/
│   ├── Services/
│   │   ├── BookService.cs
│   │   ├── UserService.cs
│   │   └── LoanService.cs
│   └── README.md
├── Library.ConsoleApp/
│   ├── Program.cs
│   └── README.md
├── Library.Tests/
│   ├── BookServiceTests.cs
│   ├── UserServiceTests.cs
│   ├── LoanServiceTests.cs
│   └── README.md
└── README.md
```

---

## 1. `Library.Core`

### `README.md`

```markdown
# Library.Core

Содержит сущности, интерфейсы репозиториев и фабрики.

## Сборка

```bash
dotnet build
```
```

### `Entities/Book.cs`

```csharp
namespace Library.Core.Entities;

/// <summary>
/// Книга.
/// </summary>
public sealed class Book
{
    public int Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }

    public Book(int id, string title, string author, int year)
    {
        Id = id;
        Title = title;
        Author = author;
        Year = year;
    }
}
```

### `Entities/User.cs`

```csharp
namespace Library.Core.Entities;

/// <summary>
/// Пользователь.
/// </summary>
public sealed class User
{
    public int Id { get; init; }
    public string Name { get; set; } = string.Empty;

    public User(int id, string name)
    {
        Id = id;
        Name = name;
    }
}
```

### `Entities/Loan.cs`

```csharp
namespace Library.Core.Entities;

/// <summary>
/// Заём книги пользователем.
/// </summary>
public sealed class Loan
{
    public int Id { get; init; }
    public int BookId { get; init; }
    public int UserId { get; init; }
    public DateTime LoanDate { get; init; }

    public Loan(int id, int bookId, int userId, DateTime loanDate)
    {
        Id = id;
        BookId = bookId;
        UserId = userId;
        LoanDate = loanDate;
    }
}
```

### `Interfaces/IBookRepository.cs`

```csharp
using Library.Core.Entities;

namespace Library.Core.Interfaces;

/// <summary>
/// Репозиторий книг.
/// </summary>
public interface IBookRepository
{
    IEnumerable<Book> GetAll();
    Book? GetById(int id);
    Book Add(Book book);
    void Update(Book book);
    void Delete(int id);
    event EventHandler<BookChangedEventArgs> BookChanged;
}
```

### `Interfaces/IUserRepository.cs`

```csharp
using Library.Core.Entities;

namespace Library.Core.Interfaces;

/// <summary>
/// Репозиторий пользователей.
/// </summary>
public interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetById(int id);
    User Add(User user);
    void Update(User user);
    void Delete(int id);
    event EventHandler<UserChangedEventArgs> UserChanged;
}
```

### `Interfaces/ILoanRepository.cs`

```csharp
using Library.Core.Entities;

namespace Library.Core.Interfaces;

/// <summary>
/// Репозиторий заёмов.
/// </summary>
public interface ILoanRepository
{
    IEnumerable<Loan> GetAll();
    Loan? GetById(int id);
    Loan Add(Loan loan);
    void Delete(int id);
    IEnumerable<Loan> GetActiveLoansByBookId(int bookId);
    event EventHandler<LoanChangedEventArgs> LoanChanged;
}
```

### `Factories/BookFactory.cs`

```csharp
using Library.Core.Entities;

namespace Library.Core.Factories;

/// <summary>
/// Фабрика для создания книг.
/// </summary>
public static class BookFactory
{
    public static Book Create(string title, string author, int year)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author is required", nameof(author));
        if (year <= 0)
            throw new ArgumentException("Year must be positive", nameof(year));

        // Id will be assigned by repository
        return new Book(0, title, author, year);
    }
}
```

### `Factories/UserFactory.cs`

```csharp
using Library.Core.Entities;

namespace Library.Core.Factories;

/// <summary>
/// Фабрика для создания пользователей.
/// </summary>
public static class UserFactory
{
    public static User Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        return new User(0, name);
    }
}
```

### `Factories/LoanFactory.cs`

```csharp
using Library.Core.Entities;

namespace Library.Core.Factories;

/// <summary>
/// Фабрика для создания заёмов.
/// </summary>
public static class LoanFactory
{
    public static Loan Create(int bookId, int userId, DateTime loanDate)
    {
        if (bookId <= 0)
            throw new ArgumentException("BookId must be positive", nameof(bookId));
        if (userId <= 0)
            throw new ArgumentException("UserId must be positive", nameof(userId));

        return new Loan(0, bookId, userId, loanDate);
    }
}
```

---

## 2. `Library.Infrastructure`

### `README.md`

```markdown
# Library.Infrastructure

Реализации репозиториев: в памяти и в JSON‑файлах.

## Сборка

```bash
dotnet build
```
```

### `Repositories/InMemoryBookRepository.cs`

```csharp
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.Infrastructure.Repositories;

/// <summary>
/// Репозиторий книг в памяти.
/// </summary>
public sealed class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books = new();
    private int _nextId = 1;

    public event EventHandler<BookChangedEventArgs>? BookChanged;

    public IEnumerable<Book> GetAll() => _books;

    public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

    public Book Add(Book book)
    {
        var newBook = new Book(_nextId++, book.Title, book.Author, book.Year);
        _books.Add(newBook);
        BookChanged?.Invoke(this, new BookChangedEventArgs(newBook, ChangeType.Added));
        return newBook;
    }

    public void Update(Book book)
    {
        var idx = _books.FindIndex(b => b.Id == book.Id);
        if (idx < 0) throw new InvalidOperationException("Book not found");
        _books[idx] = book;
        BookChanged?.Invoke(this, new BookChangedEventArgs(book, ChangeType.Updated));
    }

    public void Delete(int id)
    {
        var book = GetById(id);
        if (book == null) throw new InvalidOperationException("Book not found");
        _books.Remove(book);
        BookChanged?.Invoke(this, new BookChangedEventArgs(book, ChangeType.Deleted));
    }
}
```

### `Repositories/InMemoryUserRepository.cs`

```csharp
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.Infrastructure.Repositories;

/// <summary>
/// Репозиторий пользователей в памяти.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public event EventHandler<UserChangedEventArgs>? UserChanged;

    public IEnumerable<User> GetAll() => _users;

    public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public User Add(User user)
    {
        var newUser = new User(_nextId++, user.Name);
        _users.Add(newUser);
        UserChanged?.Invoke(this, new UserChangedEventArgs(newUser, ChangeType.Added));
        return newUser;
    }

    public void Update(User user)
    {
        var idx = _users.FindIndex(u => u.Id == user.Id);
        if (idx < 0) throw new InvalidOperationException("User not found");
        _users[idx] = user;
        UserChanged?.Invoke(this, new UserChangedEventArgs(user, ChangeType.Updated));
    }

    public void Delete(int id)
    {
        var user = GetById(id);
        if (user == null) throw new InvalidOperationException("User not found");
        _users.Remove(user);
        UserChanged?.Invoke(this, new UserChangedEventArgs(user, ChangeType.Deleted));
    }
}
```

### `Repositories/InMemoryLoanRepository.cs`

```csharp
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.Infrastructure.Repositories;

/// <summary>
/// Репозиторий заёмов в памяти.
/// </summary>
public sealed class InMemoryLoanRepository : ILoanRepository
{
    private readonly List<Loan> _loans = new();
    private int _nextId = 1;

    public event EventHandler<LoanChangedEventArgs>? LoanChanged;

    public IEnumerable<Loan> GetAll() => _loans;

    public Loan? GetById(int id) => _loans.FirstOrDefault(l => l.Id == id);

    public Loan Add(Loan loan)
    {
        var newLoan = new Loan(_nextId++, loan.BookId, loan.UserId, loan.LoanDate);
        _loans.Add(newLoan);
        LoanChanged?.Invoke(this, new LoanChangedEventArgs(newLoan, ChangeType.Added));
        return newLoan;
    }

    public void Delete(int id)
    {
        var loan = GetById(id);
        if (loan == null) throw new InvalidOperationException("Loan not found");
        _loans.Remove(loan);
        LoanChanged?.Invoke(this, new LoanChangedEventArgs(loan, ChangeType.Deleted));
    }

    public IEnumerable<Loan> GetActiveLoansByBookId(int bookId) =>
        _loans.Where(l => l.BookId == bookId);
}
```

### `Repositories/JsonBookRepository.cs`

```csharp
using System.Text.Json;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.Infrastructure.Repositories;

/// <summary>
/// Репозиторий книг, сохраняющий данные в JSON‑файл.
/// </summary>
public sealed class JsonBookRepository : IBookRepository
{
    private readonly string _filePath;
    private readonly List<Book> _books;
    private int _nextId;

    public event EventHandler<BookChangedEventArgs>? BookChanged;

    public JsonBookRepository(string filePath)
    {
        _filePath = filePath;
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _books = JsonSerializer.Deserialize<List<Book>>(json) ?? new();
            _nextId = _books.Any() ? _books.Max(b => b.Id) + 1 : 1;
        }
        else
        {
            _books = new();
            _nextId = 1;
        }
    }

    public IEnumerable<Book> GetAll() => _books;

    public Book? GetById(int id) => _books.FirstOrDefault(b => b.Id == id);

    public Book Add(Book book)
    {
        var newBook = new Book(_nextId++, book.Title, book.Author, book.Year);
        _books.Add(newBook);
        Save();
        BookChanged?.Invoke(this, new BookChangedEventArgs(newBook, ChangeType.Added));
        return newBook;
    }

    public void Update(Book book)
    {
        var idx = _books.FindIndex(b => b.Id == book.Id);
        if (idx < 0) throw new InvalidOperationException("Book not found");
        _books[idx] = book;
        Save();
        BookChanged?.Invoke(this, new BookChangedEventArgs(book, ChangeType.Updated));
    }

    public void Delete(int id)
    {
        var book = GetById(id);
        if (book == null) throw new InvalidOperationException("Book not found");
        _books.Remove(book);
        Save();
        BookChanged?.Invoke(this, new BookChangedEventArgs(book, ChangeType.Deleted));
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_books, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
```

### `Repositories/JsonUserRepository.cs`

```csharp
using System.Text.Json;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.Infrastructure.Repositories;

/// <summary>
/// Репозиторий пользователей, сохраняющий данные в JSON‑файл.
/// </summary>
public sealed class JsonUserRepository : IUserRepository
{
    private readonly string _filePath;
    private readonly List<User> _users;
    private int _nextId;

    public event EventHandler<UserChangedEventArgs>? UserChanged;

    public JsonUserRepository(string filePath)
    {
        _filePath = filePath;
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _users = JsonSerializer.Deserialize<List<User>>(json) ?? new();
            _nextId = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
        }
        else
        {
            _users = new();
            _nextId = 1;
        }
    }

    public IEnumerable<User> GetAll() => _users;

    public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public User Add(User user)
    {
        var newUser = new User(_nextId++, user.Name);
        _users.Add(newUser);
        Save();
        UserChanged?.Invoke(this, new UserChangedEventArgs(newUser, ChangeType.Added));
        return newUser;
    }

    public void Update(User user)
    {
        var idx = _users.FindIndex(u => u.Id == user.Id);
        if (idx < 0) throw new InvalidOperationException("User not found");
        _users[idx] = user;
        Save();
        UserChanged?.Invoke(this, new UserChangedEventArgs(user, ChangeType.Updated));
    }

    public void Delete(int id)
    {
        var user = GetById(id);
        if (user == null) throw new InvalidOperationException("User not found");
        _users.Remove(user);
        Save();
        UserChanged?.Invoke(this, new UserChangedEventArgs(user, ChangeType.Deleted));
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
```

### `Repositories/JsonLoanRepository.cs`

```csharp
using System.Text.Json;
using Library.Core.Entities;
using Library.Core.Interfaces;

namespace Library.Infrastructure.Repositories;

/// <summary>
/// Репозиторий заёмов, сохраняющий данные в JSON‑файл.
/// </summary>
public sealed class JsonLoanRepository : ILoanRepository
{
    private readonly string _filePath;
    private readonly List<Loan> _loans;
    private int _nextId;

    public event EventHandler<LoanChangedEventArgs>? LoanChanged;

    public JsonLoanRepository(string filePath)
    {
        _filePath = filePath;
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _loans = JsonSerializer.Deserialize<List<Loan>>(json) ?? new();
            _nextId = _loans.Any() ? _loans.Max(l => l.Id) + 1 : 1;
        }
        else
        {
            _loans = new();
            _nextId = 1;
        }
    }

    public IEnumerable<Loan> GetAll() => _loans;

    public Loan? GetById(int id) => _loans.FirstOrDefault(l => l.Id == id);

    public Loan Add(Loan loan)
    {
        var newLoan = new Loan(_nextId++, loan.BookId, loan.UserId, loan.LoanDate);
        _loans.Add(newLoan);
        Save();
        LoanChanged?.Invoke(this, new LoanChangedEventArgs(newLoan, ChangeType.Added));
        return newLoan;
    }

    public void Delete(int id)
    {
        var loan = GetById(id);
        if (loan == null) throw new InvalidOperationException("Loan not found");
        _loans.Remove(loan);
        Save();
        LoanChanged?.Invoke(this, new LoanChangedEventArgs(loan, ChangeType.Deleted));
    }

    public IEnumerable<Loan> GetActiveLoansByBookId(int bookId) =>
        _loans.Where(l => l.BookId == bookId);

    private void Save()
    {
        var json = JsonSerializer.Serialize(_loans, new
```

```json
{
  "subject_domain": "Больница",
  "algorithmic_requirements": "бинарный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 5 (ID: 1558)

- Title: Геометрический каталог: Лабораторная работа 1 – вариант 5
- Fingerprint: `lab459-v5-a3581f2b2c78`

```markdown
# Лабораторная работа 1  
## Объектно‑ориентированное проектирование и программирование  

### Цель лабораторной работы  
Разработать рабочий прототип ключевых модулей небольшого программного продукта, демонстрирующего применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм), а также базовых паттернов проектирования (Factory, Repository, Observer).  

---  

### Задача  
Создать консольное приложение **«Библиотечный каталог»**, которое реализует следующие функциональные блоки:  

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы и реализации `IBookRepository`, `IUserRepository`, `ILoanRepository` (с хранением данных в памяти и в файле JSON).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`, которые используют репозитории и реализуют операции CRUD и бизнес‑правила (например, нельзя одновремено одолжить книгу, если она уже занята).  
4. **Модуль пользовательского интерфейса** – консольный UI, позволяющий выполнять команды:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих ключевые сценарии работы сервисов.  

---  

### Вход/Выход (контракт)  

| Вход | Описание | Формат | Пример |
|------|----------|--------|--------|
| Команда консоли | Текстовая команда, разделённая пробелами | `string` | `add book` |
| Параметры команды | Дополнительные аргументы, разделённые пробелами | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Описание | Формат | Пример |
|-------|----------|--------|--------|
| Сообщения об успехе/ошибках | Текстовое сообщение, выводимое в консоль | `string` | `Книга добавлена: ID=5` |
| Список сущностей | Текстовое представление списка | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

---  

### Требования  

1. **Язык и среда**  
   - Использовать язык C# (.NET 8.0+).  
   - Проект должен компилироваться без ошибок в командной строке `dotnet build`.  

2. **Структура проекта**  
   - Сформировать несколько проектов:  
     - `Library.Core` – сущности и интерфейсы.  
     - `Library.Infrastructure` – реализации репозиториев.  
     - `Library.Application` – сервисы бизнес‑логики.  
     - `Library.ConsoleApp` – пользовательский интерфейс.  
     - `Library.Tests` – unit‑тесты.  

3. **Принципы ООП**  
   - Каждый класс должен иметь чётко определённую ответственность.  
   - Использовать инкапсуляцию: поля – `private`, доступ через свойства/методы.  
   - Применить наследование/полиморфизм там, где это оправдано (например, разные типы пользователей).  

4. **Паттерны проектирования**  
   - **Factory** – для создания объектов `Book`, `User`, `Loan`.  
   - **Repository** – абстракция доступа к данным.  
   - **Observer** – оповещение UI о изменениях в данных (можно реализовать простым событием).  

5. **Постоянное хранение**  
   - Реализовать сохранение и загрузку данных из файлов JSON (`books.json`, `users.json`, `loans.json`).  
   - При запуске приложения данные загружаются, при завершении – сохраняются.  

6. **Обработка ошибок**  
   - Все публичные методы сервисов должны бросать исключения `ArgumentException`, `InvalidOperationException` при некорректных входных данных.  
   - UI должно перехватывать исключения и выводить понятные сообщения.  

7. **Тесты**  
   - Минимум 10 unit‑тестов, покрывающих:  
     - Добавление/удаление/обновление сущностей.  
     - Проверка бизнес‑правил (например, невозможность одолжить уже занятую книгу).  
     - Сохранение/загрузка из файлов.  

8. **Документация**  
   - В каждом проекте должен быть файл `README.md`, описывающий назначение и как собрать/запустить.  
   - В коде использовать XML‑комментарии для публичных API.  

9. **Код‑стайл**  
   - Следовать официальному стилю C# (Microsoft).  
   - Код должен быть читаемым, без лишних комментариев.  

10. **Ограничения**  
    - Не использовать сторонние ORM‑библиотеки (Entity Framework, Dapper).  
    - Все операции с файлами должны быть синхронными.  

---  

### Проверка кода  

- **Сборка**: перед сдачей убедитесь, что проект собирается без ошибок:  
  ```bash
  dotnet build
  ```  
- **Тесты**: все unit‑тесты должны проходить:  
  ```bash
  dotnet test
  ```  
- **Запуск**: приложение должно корректно запускаться и обрабатывать команды:  
  ```bash
  dotnet run --project Library.ConsoleApp
  ```  
- **CI** (необязательно): можно настроить GitHub Actions или GitLab CI, чтобы автоматически проверять сборку и тесты при каждом коммите.  

---  

### Как сдавать  
1. Создать репозиторий GitHub (или GitLab) с описанным проектом.  
2. В корне репозитория разместить файл `README.md` с инструкцией по сборке и запуску.  
3. Убедиться, что `dotnet build` и `dotnet test` проходят без ошибок.  
4. Отправить ссылку на репозиторий преподавателю.  

---
```

```json
{
  "subject_domain": "Геометрические фигуры",
  "algorithmic_requirements": "линейный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 6 (ID: 1559)

- Title: Бакалейный каталог – вариант 6
- Fingerprint: `lab459-v6-c79cd7457e71`

```markdown
# Библиотечный каталог – вариант 6

## Цель лабораторной работы  
Разработать рабочий прототип ключевых модулей небольшого программного продукта, демонстрирующего применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм) и базовых паттернов проектирования (Factory, Repository, Observer).

## Задача  
Создать консольное приложение **«Библиотечный каталог»**, которое реализует следующие функциональные блоки:

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы и реализации `IBookRepository`, `IUserRepository`, `ILoanRepository` (с хранением данных в памяти и в файле JSON).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`, которые используют репозитории и реализуют операции CRUD и бизнес‑правила (например, нельзя одновремено одолжить книгу, если она уже занята).  
4. **Модуль пользовательского интерфейса** – консольный UI, позволяющий выполнять команды:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих ключевые сценарии работы сервисов.

## Вход/выход (контракт)  

| Вход | Описание | Формат | Пример |
|------|----------|--------|--------|
| Команда консоли | Текстовая команда, разделённая пробелами | `string` | `add book` |
| Параметры команды | Дополнительные аргументы, разделённые пробелами | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Описание | Формат | Пример |
|-------|----------|--------|--------|
| Сообщения об успехе/ошибках | Текстовое сообщение, выводимое в консоль | `string` | `Книга добавлена: ID=5` |
| Список сущностей | Текстовое представление списка | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

## Требования  

1. **Язык и среда**  
   - Использовать язык C# (.NET 8.0+).  
   - Проект должен компилироваться без ошибок командой `dotnet build`.  

2. **Структура проекта**  
   - `Library.Core` – сущности и интерфейсы.  
   - `Library.Infrastructure` – реализации репозиториев.  
   - `Library.Application` – сервисы бизнес‑логики.  
   - `Library.ConsoleApp` – пользовательский интерфейс.  
   - `Library.Tests` – unit‑тесты.  

3. **Принципы ООП**  
   - Каждый класс имеет чётко определённую ответственность.  
   - Инкапсуляция: поля – `private`, доступ через свойства/методы.  
   - Наследование/полиморфизм применяются там, где это оправдано (например, разные типы пользователей).  

4. **Паттерны проектирования**  
   - **Factory** – для создания объектов `Book`, `User`, `Loan`.  
   - **Repository** – абстракция доступа к данным.  
   - **Observer** – оповещение UI о изменениях в данных (реализовано через событие).  

5. **Постоянное хранение**  
   - Сохранение и загрузка данных из файлов JSON (`books.json`, `users.json`, `loans.json`).  
   - При запуске приложение загружает данные, при завершении – сохраняет.  

6. **Обработка ошибок**  
   - Все публичные методы сервисов бросают `ArgumentException`, `InvalidOperationException` при некорректных входных данных.  
   - UI перехватывает исключения и выводит понятные сообщения.  

7. **Тесты**  
   - Минимум 10 unit‑тестов, покрывающих:  
     - Добавление/удаление/обновление сущностей.  
     - Проверка бизнес‑правил (например, невозможность одолжить уже занятую книгу).  
     - Сохранение/загрузка из файлов.  

8. **Документация**  
   - В каждом проекте файл `README.md`, описывающий назначение и как собрать/запустить.  
   - В коде XML‑комментарии для публичных API.  

9. **Код‑стайл**  
   - Следовать официальному стилю C# (Microsoft).  
   - Код читаемый, без лишних комментариев.  

10. **Ограничения**  
    - Не использовать сторонние ORM‑библиотеки (Entity Framework, Dapper).  
    - Все операции с файлами синхронные.  

## Как сдавать  
1. Создать репозиторий GitHub (или GitLab) с описанным проектом.  
2. В корне репозитория разместить файл `README.md` с инструкцией по сборке и запуску.  
3. Отправить ссылку на репозиторий преподавателю.  

---  

**Замечание**: данный вариант полностью соответствует требованиям мастер‑задания и реализует все требуемые модули, паттерны и функциональные команды.
```

```json
{
  "subject_domain": "Пекарня",
  "algorithmic_requirements": "сортировка вставками"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 7 (ID: 1560)

- Title: Музейный каталог экспонатов – вариант 7
- Fingerprint: `lab459-v7-559db2d7c952`

```markdown
# Лабораторная работа 1  
## Объектно‑ориентированное проектирование и программирование  

### Цель лабораторной работы  
Разработать рабочий прототип ключевых модулей небольшого программного продукта, демонстрирующего применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм) и базовых паттернов проектирования (Factory, Repository, Observer).  

---

### Задача  
Создать консольное приложение **«Библиотечный каталог»**, которое реализует следующие функциональные блоки:  

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы и реализации `IBookRepository`, `IUserRepository`, `ILoanRepository` (с хранением данных в памяти и в файле JSON).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`, которые используют репозитории и реализуют операции CRUD и бизнес‑правила (например, нельзя одновремено одолжить книгу, если она уже занята).  
4. **Модуль пользовательского интерфейса** – консольный UI, позволяющий выполнять команды:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих ключевые сценарии работы сервисов.  

---

### Вход/Выход (контракт)  

| Вход | Описание | Формат | Пример |
|------|----------|--------|--------|
| Команда консоли | Текстовая команда, разделённая пробелами | `string` | `add book` |
| Параметры команды | Дополнительные аргументы, разделённые пробелами | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Описание | Формат | Пример |
|-------|----------|--------|--------|
| Сообщения об успехе/ошибках | Текстовое сообщение, выводимое в консоль | `string` | `Книга добавлена: ID=5` |
| Список сущностей | Текстовое представление списка | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

---

### Требования  

1. **Язык и среда**  
   - Использовать язык C# (.NET 8.0+).  
   - Проект должен компилироваться без ошибок в командной строке `dotnet build`.  

2. **Структура проекта**  
   - `Library.Core` – сущности и интерфейсы.  
   - `Library.Infrastructure` – реализации репозиториев.  
   - `Library.Application` – сервисы бизнес‑логики.  
   - `Library.ConsoleApp` – пользовательский интерфейс.  
   - `Library.Tests` – unit‑тесты.  

3. **Принципы ООП**  
   - Каждый класс имеет чётко определённую ответственность.  
   - Инкапсуляция: поля – `private`, доступ через свойства/методы.  
   - Наследование/полиморфизм применяются там, где оправдано (например, разные типы пользователей).  

4. **Паттерны проектирования**  
   - **Factory** – для создания объектов `Book`, `User`, `Loan`.  
   - **Repository** – абстракция доступа к данным.  
   - **Observer** – оповещение UI о изменениях в данных (можно реализовать простым событием).  

5. **Постоянное хранение**  
   - Реализовать сохранение и загрузку данных из файлов JSON (`books.json`, `users.json`, `loans.json`).  
   - При запуске приложения данные загружаются, при завершении – сохраняются.  

6. **Обработка ошибок**  
   - Все публичные методы сервисов должны бросать исключения `ArgumentException`, `InvalidOperationException` при некорректных входных данных.  
   - UI должно перехватывать исключения и выводить понятные сообщения.  

7. **Тесты**  
   - Минимум 10 unit‑тестов, покрывающих:  
     - Добавление/удаление/обновление сущностей.  
     - Проверка бизнес‑правил (например, невозможность одолжить уже занятую книгу).  
     - Сохранение/загрузка из файлов.  

8. **Документация**  
   - В каждом проекте должен быть файл `README.md`, описывающий назначение и как собрать/запустить.  
   - В коде использовать XML‑комментарии для публичных API.  

9. **Код‑стайл**  
   - Следовать официальному стилю C# (Microsoft).  
   - Код должен быть читаемым, без лишних комментариев.  

10. **Ограничения**  
    - Не использовать сторонние ORM‑библиотеки (Entity Framework, Dapper).  
    - Все операции с файлами должны быть синхронными.  

---

### Как сдавать  
1. Создать репозиторий GitHub (или GitLab) с описанным проектом.  
2. В корне репозитория разместить файл `README.md` с инструкцией по сборке и запуску.  
3. Отправить ссылку на репозиторий преподавателю.  

---
```

```json
{
  "subject_domain": "Музей",
  "algorithmic_requirements": "сортировка слиянием"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 8 (ID: 1561)

- Title: Фитнес‑центр: управление членством, тренировками и расписанием
- Fingerprint: `lab459-v8-c736910ec630`

```markdown
# Библиотечный каталог  
## Управление книгами, пользователями и займами  

---

## Цель лабораторной работы  
Создать консольное приложение **«Библиотечный каталог»**, демонстрирующее применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм) и базовых паттернов проектирования (Factory, Repository, Observer) в контексте небольшого библиотечного сервиса.  

---

## Ключевые требования  

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы `IBookRepository`, `IUserRepository`, `ILoanRepository` и их реализации в памяти и в JSON‑файлах (`books.json`, `users.json`, `loans.json`).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`.  
   *Бизнес‑правила:*  
   - Книга может быть одолжена только один раз одновременно.  
   - Пользователь может иметь не более 5 одолженных книг.  
   - При возврате книги освобождается место.  
4. **Модуль пользовательского интерфейса** – консольный UI с командами:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих CRUD‑операции, бизнес‑правила и сохранение/загрузку данных.  
6. **Паттерны проектирования**  
   - **Factory** – `EntityFactory` создаёт объекты `Book`, `User`, `Loan`.  
   - **Repository** – абстракция доступа к данным.  
   - **Observer** – событие `DataChanged` оповещает UI о добавлении/удалении/обновлении сущностей.  
7. **Постоянное хранение** – синхронная работа с JSON‑файлами.  
8. **Обработка ошибок** – публичные методы сервисов бросают `ArgumentException`, `InvalidOperationException`.  
9. **Документация** – каждый проект содержит `README.md` и XML‑комментарии к публичному API.  
10. **Код‑стайл** – соблюдение официального стиля C# (Microsoft).  

---

## Структура проекта  

```
Library.Core          – сущности и интерфейсы  
Library.Infrastructure – реализации репозиториев (in‑memory, JSON)  
Library.Application   – сервисы бизнес‑логики  
Library.ConsoleApp    – пользовательский интерфейс  
Library.Tests         – unit‑тесты  
```

---

## Пример команды  

```text
add book "The Hobbit" "J.R.R. Tolkien" 1937
add user "Иван Иванов" 28
loan book 1 1
list loans
return book 1
```

---

## Как сдавать  

1. Создать репозиторий GitHub (или GitLab) с описанным проектом.  
2. В корне репозитория разместить файл `README.md` с инструкцией по сборке и запуску.  
3. Отправить ссылку на репозиторий преподавателю.  

---
```

```json
{
  "subject_domain": "Фитнес‑центр",
  "algorithmic_requirements": "алгоритм Дейкстры"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 9 (ID: 1562)

- Title: Парк развлечений – каталог аттракционов и билетов
- Fingerprint: `lab459-v9-65ffb8000e64`

```markdown
# Лабораторная работа 1  
## Объектно‑ориентированное проектирование и программирование  

### Цель лабораторной работы  
Разработать рабочий прототип ключевых модулей небольшого программного продукта, демонстрирующего применение принципов объектно‑ориентированного проектирования (SOLID, инкапсуляция, наследование, полиморфизм) и базовых паттернов проектирования (Factory, Repository, Observer).  

---

### Задача  
Создать консольное приложение **«Библиотечный каталог»**, которое реализует следующие функциональные блоки:  

1. **Модуль сущностей** – классы `Book`, `User`, `Loan`.  
2. **Модуль репозиториев** – интерфейсы и реализации `IBookRepository`, `IUserRepository`, `ILoanRepository` (с хранением данных в памяти и в файле JSON).  
3. **Модуль бизнес‑логики** – сервисы `BookService`, `UserService`, `LoanService`, которые используют репозитории и реализуют операции CRUD и бизнес‑правила (например, нельзя одновремено одолжить книгу, если она уже занята).  
4. **Модуль пользовательского интерфейса** – консольный UI, позволяющий выполнять команды:  
   - `add book`, `list books`, `update book`, `delete book`  
   - `add user`, `list users`, `update user`, `delete user`  
   - `loan book`, `return book`, `list loans`  
5. **Модуль тестирования** – набор unit‑тестов, покрывающих ключевые сценарии работы сервисов.  

---

### Вход/Выход (контракт)  

| Вход | Описание | Формат | Пример |
|------|----------|--------|--------|
| Команда консоли | Текстовая команда, разделённая пробелами | `string` | `add book` |
| Параметры команды | Дополнительные аргументы, разделённые пробелами | `string[]` | `add book "The Hobbit" "J.R.R. Tolkien" 1937` |

| Выход | Описание | Формат | Пример |
|-------|----------|--------|--------|
| Сообщения об успехе/ошибках | Текстовое сообщение, выводимое в консоль | `string` | `Книга добавлена: ID=5` |
| Список сущностей | Текстовое представление списка | `string` | `ID: 5, Title: The Hobbit, Author: J.R.R. Tolkien, Year: 1937` |

---

### Требования  

1. **Язык и среда**  
   - Использовать язык C# (.NET 8.0+).  
   - Проект должен компилироваться без ошибок в командной строке `dotnet build`.  

2. **Структура проекта**  
   - `Library.Core` – сущности и интерфейсы.  
   - `Library.Infrastructure` – реализации репозиториев.  
   - `Library.Application` – сервисы бизнес‑логики.  
   - `Library.ConsoleApp` – пользовательский интерфейс.  
   - `Library.Tests` – unit‑тесты.  

3. **Принципы ООП**  
   - Каждый класс должен иметь чётко определённую ответственность.  
   - Использовать инкапсуляцию: поля – `private`, доступ через свойства/методы.  
   - Применить наследование/полиморфизм там, где это оправдано (например, разные типы пользователей).  

4. **Паттерны проектирования**  
   - **Factory** – для создания объектов `Book`, `User`, `Loan`.  
   - **Repository** – абстракция доступа к данным.  
   - **Observer** – оповещение UI о изменениях в данных (можно реализовать простым событием).  

5. **Постоянное хранение**  
   - Реализовать сохранение и загрузку данных из файлов JSON (`books.json`, `users.json`, `loans.json`).  
   - При запуске приложения данные загружаются, при завершении – сохраняются.  

6. **Обработка ошибок**  
   - Все публичные методы сервисов должны бросать исключения `ArgumentException`, `InvalidOperationException` при некорректных входных данных.  
   - UI должно перехватывать исключения и выводить понятные сообщения.  

7. **Тесты**  
   - Минимум 10 unit‑тестов, покрывающих:  
     - Добавление/удаление/обновление сущностей.  
     - Проверка бизнес‑правил (например, невозможность одолжить уже занятую книгу).  
     - Сохранение/загрузка из файлов.  

8. **Документация**  
   - В каждом проекте должен быть файл `README.md`, описывающий назначение и как собрать/запустить.  
   - В коде использовать XML‑комментарии для публичных API.  

9. **Код‑стайл**  
   - Следовать официальному стилю C# (Microsoft).  
   - Код должен быть читаемым, без лишних комментариев.  

10. **Ограничения**  
    - Не использовать сторонние ORM‑библиотеки (Entity Framework, Dapper).  
    - Все операции с файлами должны быть синхронными.  

---

### Как сдавать  
1. Создать репозиторий GitHub (или GitLab) с описанным проектом.  
2. В корне репозитория разместить файл `README.md` с инструкцией по сборке и запуску.  
3. Отправить ссылку на репозиторий преподавателю.  

---
```

```json
{
  "subject_domain": "Парк развлечений",
  "algorithmic_requirements": "алгоритм Хаффмана"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```


### Lab 2

- Planned title: Лабораторная работа 2: Тестирование и отладка
- Created lab id: `460`
- Master updated: `False`
- Master review comment: 
- Variants total: `6`
- Verification retries: `1`
- Regeneration retries: `0`
- Final status: **Passed**

#### Lab Initial Description

```markdown
Лабораторная работа 2 по дисциплине "Объектно-ориентированное проектирование и программирование". Тематический фокус: проектирование тестов и поиск дефектов. Сформулируй уникальное задание, отличающееся от других лабораторных работ этой дисциплины.
```

#### Master Assignment (Final)

```markdown
# Лабораторная работа 2  
## Объектно‑ориентированное проектирование и программирование  
### Тема: Проектирование тестов и поиск дефектов

---

## Цель работы  
Научиться систематически разрабатывать наборы модульных и интеграционных тестов для объектно‑ориентированных программ, а также выявлять и фиксировать дефекты, используя принципы тест‑драйвен‑девелопмента (TDD) и статического анализа кода.

---

## Задание  
Выполнить проектирование и реализацию тестовой среды для заданного программного модуля (класс/модуль), а также провести анализ кода и исправить обнаруженные дефекты.  
В процессе работы студент должен:

1. **Анализ требований** – определить функциональные и нефункциональные требования к модулю.
2. **Разработка тест‑плана** – сформулировать набор тест‑кейсов, покрывающих как позитивные, так и негативные сценарии.
3. **Реализация тестов** – написать модульные и интеграционные тесты, используя выбранный фреймворк (JUnit, pytest, unittest и т.п.).
4. **Тест‑драйвен‑девелопмент** – при необходимости добавить недостающий функционал, чтобы пройти все тесты.
5. **Статический анализ** – выполнить анализ кода с помощью инструментов (SonarQube, pylint, cppcheck и т.п.) и исправить найденные дефекты.
6. **Документация** – подготовить отчёт, включающий описание тест‑плана, результаты тестирования, список исправленных дефектов и выводы.

---

## Входные данные  
- Исходный код модуля (или ссылка на репозиторий).
- Техническое задание, описывающее ожидаемое поведение модуля.

## Выходные данные  
- Набор тестовых файлов, полностью покрывающих функциональность модуля.
- Отчёт о выполнении тестов (результаты, покрытие, статистика).
- Список исправленных дефектов с описанием изменений.
- Краткое резюме о применённых методах тестирования и обнаруженных проблемах.

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Особые случаи |
|---|----------------|-------------|---------------|
| 1 | Тест‑план | Должен покрывать минимум 80 % кода модуля | Включить как позитивные, так и негативные сценарии |
| 2 | Модульные тесты | Использовать стандартный фреймворк (JUnit, pytest, unittest и т.п.) | Тесты должны быть независимыми и воспроизводимыми |
| 3 | Интеграционные тесты | Проверять взаимодействие модуля с внешними компонентами (если применимо) | Учитывать задержки и возможные ошибки сети |
| 4 | TDD‑пакет | При добавлении нового функционала тесты должны быть написаны до кода | Код должен компилироваться и проходить все тесты |
| 5 | Статический анализ | Использовать инструмент, поддерживающий выбранный язык (SonarQube, pylint, cppcheck и т.п.) | Исправлять все критические и важные предупреждения |
| 6 | Документация | Отчёт должен содержать: описание тест‑плана, таблицу результатов, список дефектов и их исправлений | Отчёт оформлен в Markdown/LaTeX (по выбору студента) |
| 7 | Репозиторий | Код и тесты должны храниться в системе контроля версий (Git) | Включить README с инструкциями по запуску тестов |
| 8 | Портируемость | Тесты должны запускаться на любой машине с установленным интерпретатором/компилятором | Не использовать специфичные для ОС пути/команды |
| 9 | Эффективность | Время выполнения тестов не должно превышать 5 секунд на стандартном ноутбуке | При превышении времени необходимо оптимизировать тесты |
|10 | Безопасность | Тесты не должны раскрывать чувствительные данные (логины, пароли) | Использовать фиктивные данные и мок‑объекты |

---

## Критерии оценки  

1. **Покрытие кода** – ≥ 80 % (модульные и интеграционные тесты).  
2. **Качество тестов** – корректность, независимость, читаемость.  
3. **Исправление дефектов** – полнота и обоснованность изменений.  
4. **Документация** – полнота, структурированность, наличие выводов.  
5. **Соблюдение требований** – отсутствие запрещённых элементов (вариантов, списков и т.п.).  

---
```
- Applied variation methods:
  - `subject_domain` (id=1, preserve=True)
  - `algorithmic_requirements` (id=4, preserve=True)
- Verification summary:
  - Variant `1563`: passed=True, score=10, issues=0
  - Variant `1564`: passed=True, score=10, issues=0
  - Variant `1565`: passed=True, score=10, issues=0
  - Variant `1566`: passed=True, score=9, issues=0
  - Variant `1567`: passed=True, score=10, issues=0
  - Variant `1568`: passed=True, score=8, issues=1

#### Generated Variants

##### Variant 1 (ID: 1563)

- Title: Тестирование системы управления библиотекой с хешированием
- Fingerprint: `lab460-v1-b46eab7df5b0`

```markdown
## Цель работы
Научиться систематически разрабатывать наборы модульных и интеграционных тестов для объектно‑ориентированного модуля управления библиотекой, использующего хеширование для поиска и хранения данных, а также выявлять и фиксировать дефекты, применяя принципы TDD и статического анализа кода.

## Задача
Выполнить проектирование и реализацию тестовой среды для модуля **LibraryCatalog** (класс/модуль), отвечающего за хранение и поиск книг по ISBN, автору и названию. Модуль использует хеш‑таблицу для быстрого доступа к объектам Book.

## Требования к реализации
1. **Анализ требований** – определить функциональные и нефункциональные требования к модулю.
2. **Разработка тест‑плана** – сформулировать набор тест‑кейсов, покрывающих как позитивные, так и негативные сценарии.
3. **Реализация тестов** – написать модульные и интеграционные тесты, используя `pytest`.
4. **TDD‑пакет** – при необходимости добавить недостающий функционал, чтобы пройти все тесты.
5. **Статический анализ** – выполнить анализ кода с помощью `pylint` и исправить найденные дефекты.
6. **Документация** – подготовить отчёт, включающий описание тест‑плана, результаты тестирования, список исправленных дефектов и выводы.

## Тест‑план
| № | Тест‑случай | Описание | Ожидаемый результат |
|---|-------------|----------|---------------------|
| 1 | add_book_valid | Добавление книги с корректными данными | Книга успешно добавлена, возвращается `True` |
| 2 | add_book_duplicate | Добавление книги с уже существующим ISBN | Возникает `ValueError` |
| 3 | find_by_isbn_exists | Поиск книги по существующему ISBN | Возвращается объект `Book` |
| 4 | find_by_isbn_not_exists | Поиск книги по несуществующему ISBN | Возвращается `None` |
| 5 | find_by_author | Поиск всех книг конкретного автора | Возвращается список книг |
| 6 | remove_book_valid | Удаление существующей книги | Возвращается `True` |
| 7 | remove_book_not_exists | Удаление несуществующей книги | Возвращается `False` |
| 8 | hash_collision | Тест на коллизии хеш‑таблицы (модульный тест) | Коллизии корректно обрабатываются, поиск по ключу работает |
| 9 | performance_hash | Профилирование скорости поиска при 10 000 записей | Время поиска < 5 мс |

## TDD‑пакет
1. Написать тест `test_add_book_valid` → `assert catalog.add_book(book) == True`.
2. Запустить тест → не проходит, добавить метод `add_book` в `LibraryCatalog`.
3. Повторить для остальных тестов, пока все не пройдут.

## Статический анализ
- Запустить `pylint library_catalog.py`.
- Исправить предупреждения: `unused-import`, `missing-docstring`, `invalid-name`.
- Проверить, что код соответствует PEP8 и имеет покрытие >80 %.

## Документация
Отчёт оформлен в Markdown: 
- **Тест‑план** – таблица выше.
- **Результаты тестирования** – вывод `pytest -q` с покрытием.
- **Список дефектов** – таблица с номерами, описанием и изменёнными строками кода.
- **Выводы** – оценка эффективности хеш‑таблицы, выявленные узкие места.

## Итоги
После выполнения работы студент получает:
- Полный набор тестов, покрывающих 90 % кода.
- Исправленный модуль `LibraryCatalog` с хеш‑таблицей.
- Отчёт о тестировании и статическом анализе.
- Практический опыт применения TDD и статического анализа в проекте.

---
**Инструкции по запуску**
```bash
pip install pytest pylint
pytest --cov=library_catalog
pylint library_catalog.py
```

## Примечания
- Хеш‑таблица реализована как словарь Python.
- Для симуляции коллизий в тестах используется специально созданный ключ с одинаковым хеш‑значением.
- В реальном проекте можно заменить словарь на `dict` с пользовательским хеш‑функцией.
```

```json
{
  "subject_domain": "Библиотека",
  "algorithmic_requirements": "хеширование"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 2 (ID: 1564)

- Title: Тестирование кинотеатральной системы с деревом
- Fingerprint: `lab460-v2-e258861fdda5`

```markdown
## Цель работы
Научиться систематически разрабатывать наборы модульных и интеграционных тестов для объектно‑ориентированного программного модуля, использующего структуру данных дерево, а также выявлять и фиксировать дефекты, применяя принципы тест‑драйвен‑девелопмента (TDD) и статического анализа кода.

## Задание
Выполнить проектирование и реализацию тестовой среды для модуля **MovieScheduleTree** – класса, реализующего планирование сеансов кинотеатра с помощью сбалансированного двоичного дерева поиска (AVL). Модуль должен обеспечивать операции:
- добавление сеанса (дата, время, название фильма);
- удаление сеанса по ключу;
- поиск ближайшего сеанса к заданному времени;
- получение списка сеансов в диапазоне дат.

В процессе работы студент должен:
1. **Анализ требований** – определить функциональные и нефункциональные требования к модулю.
2. **Разработка тест‑плана** – сформулировать набор тест‑кейсов, покрывающих как позитивные, так и негативные сценарии.
3. **Реализация тестов** – написать модульные и интеграционные тесты, используя `pytest`.
4. **Тест‑драйвен‑девелопмент** – при необходимости добавить недостающий функционал, чтобы пройти все тесты.
5. **Статический анализ** – выполнить анализ кода с помощью `pylint` и исправить найденные дефекты.
6. **Документация** – подготовить отчёт, включающий описание тест‑плана, результаты тестирования, список исправленных дефектов и выводы.

## Входные данные
- Исходный код модуля `MovieScheduleTree` (Python 3.10+).
- Техническое задание, описывающее ожидаемое поведение модуля.

## Выходные данные
- Набор тестовых файлов (`test_movie_schedule_tree.py`), полностью покрывающих функциональность модуля.
- Отчёт о выполнении тестов (результаты, покрытие, статистика).
- Список исправленных дефектов с описанием изменений.
- Краткое резюме о применённых методах тестирования и обнаруженных проблемах.

## Требования к реализации
| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | Тест‑план | Должен покрывать минимум 80 % кода модуля | Включить как позитивные, так и негативные сценарии |
| 2 | Модульные тесты | Использовать `pytest` | Тесты должны быть независимыми и воспроизводимыми |
| 3 | Интеграционные тесты | Проверять взаимодействие модуля с внешними компонентами (например, базой данных SQLite) | Учитывать задержки и возможные ошибки сети |
| 4 | TDD‑пакет | При добавлении нового функционала тесты должны быть написаны до кода | Код должен компилироваться и проходить все тесты |
| 5 | Статический анализ | Использовать `pylint` | Исправлять все критические и важные предупреждения |
| 6 | Документация | Отчёт оформлен в Markdown | Отчёт должен содержать: описание тест‑плана, таблицу результатов, список дефектов и их исправлений |
| 7 | Репозиторий | Код и тесты должны храниться в Git | Включить README с инструкциями по запуску тестов |
| 8 | Портируемость | Тесты должны быть совместимы с любой ОС | |

## Оценка сложности
- **Уровень сложности**: medium
- **Оценка времени**: 6 часов

## Параметры варианта
- **subject_domain**: Кинотеатр
- **algorithmic_requirements**: дерево
```

```json
{
  "subject_domain": "Кинотеатр",
  "algorithmic_requirements": "дерево"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 3 (ID: 1565)

- Title: Тестирование системы управления магазином с использованием стека
- Fingerprint: `lab460-v3-a65fb5440dd3`

```markdown
## Лабораторная работа 2  
### Объектно‑ориентированное проектирование и программирование  
#### Тема: Проектирование тестов и поиск дефектов для магазина  

**Цель работы**  
Научиться систематически разрабатывать наборы модульных и интеграционных тестов для объектно‑ориентированного программного модуля, реализующего логику управления магазином, а также выявлять и фиксировать дефекты, используя принципы тест‑драйвен‑девелопмента (TDD) и статического анализа кода.  

**Задание**  
Выполнить проектирование и реализацию тестовой среды для заданного модуля «Магазин», в котором реализован стек для управления корзиной покупок. Стек должен поддерживать операции `push`, `pop`, `peek`, `isEmpty` и `size`.  

В процессе работы студент:
1. **Анализ требований** – определить функциональные и нефункциональные требования к модулю.  
2. **Разработка тест‑плана** – сформулировать набор тест‑кейсов, покрывающих как позитивные, так и негативные сценарии.  
3. **Реализация тестов** – написать модульные и интеграционные тесты, используя `pytest`.  
4. **Тест‑драйвен‑девелопмент** – при необходимости добавить недостающий функционал, чтобы пройти все тесты.  
5. **Статический анализ** – выполнить анализ кода с помощью `pylint` и исправить найденные дефекты.  
6. **Документация** – подготовить отчёт, включающий описание тест‑плана, результаты тестирования, список исправленных дефектов и выводы.  

**Входные данные**  
- Исходный код класса `ShoppingCartStack` (или ссылка на репозиторий).  
- Техническое задание, описывающее ожидаемое поведение модуля.  

**Выходные данные**  
- Набор тестовых файлов, полностью покрывающих функциональность модуля.  
- **Отчёт о выполнении тестов** (результаты, покрытие, статистика).  
- Список исправленных дефектов с описанием изменений.  
- Краткое резюме о применённых методах тестирования и обнаруженных проблемах.  

**Требования к реализации**  

| № | Что реализовать | Ограничения | Особые случаи |
|---|----------------|-------------|---------------|
| 1 | Тест‑план | Должен покрывать минимум **80 %** кода модуля | Включить как позитивные, так и негативные сценарии |
| 2 | Модульные тесты | Использовать `pytest` | Тесты должны быть независимыми и воспроизводимыми |
| 3 | Интеграционные тесты | Проверять взаимодействие модуля с внешними компонентами (если применимо) | Учитывать задержки и возможные ошибки сети |
| 4 | TDD‑пакет | При добавлении нового функционала тесты должны быть написаны до кода | Код должен компилироваться и проходить все тесты |
| 5 | Статический анализ | Использовать `pylint` | **Все критические и важные предупреждения исправлены** |
| 6 | Документация | Отчёт должен содержать: описание тест‑плана, таблицу результатов, список дефектов и их исправлений | Отчёт оформлен в Markdown |
| 7 | Репозиторий | Код и тесты должны храниться в системе контроля версий (Git) | Включить `README` с инструкциями по запуску тестов |
| 8 | Портируемость | Тесты должны быть совместимы с Python 3.8+ | |

**Требования к сложности**  
- complexity: **medium**  
- estimated_hours: **6**  

**Параметры варьирования**  
- subject_domain: **Магазин**  
- algorithmic_requirements: **стек**  

---  

> **Важно**  
> * Тесты покрывают ≥ 80 % кода.  
> * Интеграционные тесты добавлены (если стек взаимодействует с внешними компонентами).  
> * Отчёт о тестировании включён в итоговый пакет.  
> * Все критические предупреждения `pylint` исправлены и подтверждены.
```

```json
{
  "subject_domain": "Магазин",
  "algorithmic_requirements": "стек"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 4 (ID: 1566)

- Title: Тестирование системы управления больницей с бинарным поиском
- Fingerprint: `lab460-v4-420678eefbfc`

```markdown
## Лабораторная работа 2
### Объектно‑ориентированное проектирование и программирование
#### Тема: Проектирование тестов и поиск дефектов для системы управления больницей

### Цель работы
Научиться систематически разрабатывать наборы модульных и интеграционных тестов для объектно‑ориентированного программного модуля, реализующего поиск пациентов по идентификатору с помощью бинарного поиска, а также выявлять и фиксировать дефекты, используя принципы тест‑драйвен‑девелопмента (TDD) и статического анализа кода.

### Задание
Выполнить проектирование и реализацию тестовой среды для модуля **PatientRegistry** (регистратор пациентов), который хранит список пациентов в отсортированном по ID массиве и предоставляет методы:
- `add_patient(patient)` – добавление пациента;
- `find_patient_by_id(patient_id)` – поиск пациента по ID (бинарный поиск);
- `remove_patient(patient_id)` – удаление пациента.

В процессе работы студент должен:
1. **Анализ требований** – определить функциональные и нефункциональные требования к модулю.
2. **Разработка тест‑плана** – сформулировать набор тест‑кейсов, покрывающих как позитивные, так и негативные сценарии.
3. **Реализация тестов** – написать модульные и интеграционные тесты, используя `pytest`.
4. **Тест‑драйвен‑девелопмент** – при необходимости добавить недостающий функционал, чтобы пройти все тесты.
5. **Статический анализ** – выполнить анализ кода с помощью `pylint` и исправить найденные дефекты.
6. **Документация** – подготовить отчёт, включающий описание тест‑плана, результаты тестирования, список исправленных дефектов и выводы.

### Требования к реализации
| № | Что реализовать | Ограничения | Особые случаи |
|---|----------------|-------------|---------------|
| 1 | Тест‑план | Должен покрывать минимум 80 % кода модуля | Включить как позитивные, так и негативные сценарии |
| 2 | Модульные тесты | Использовать `pytest` | Тесты должны быть независимыми и воспроизводимыми |
| 3 | Интеграционные тесты | Проверять взаимодействие модуля с внешним хранилищем (SQLite) | Учитывать задержки и возможные ошибки сети |
| 4 | TDD‑пакет | При добавлении нового функционала тесты должны быть написаны до кода | Код должен компилироваться и проходить все тесты |
| 5 | Статический анализ | Использовать `pylint` | Исправлять все критические и важные предупреждения |
| 6 | Документация | Отчёт должен содержать: описание тест‑плана, таблицу результатов, список дефектов и их исправлений | Отчёт оформлен в Markdown |
| 7 | Репозиторий | Код и тесты должны храниться в Git | Включить README с инструкциями по запуску тестов |
| 8 | Портируемость | Тесты должны работать на Windows, macOS и Linux | |

### Ожидаемые результаты
- Набор тестовых файлов, полностью покрывающих функциональность модуля.
- Отчёт о выполнении тестов (результаты, покрытие, статистика).
- Список исправленных дефектов с описанием изменений.
- Краткое резюме о применённых методах тестирования и обнаруженных проблемах.

### Сроки и оценка
- Оценка сложности: **medium**.
- Оценка времени: **4–8** часов.

---

**Параметры варианта**
- **subject_domain**: Больница
- **algorithmic_requirements**: бинарный поиск
```

```json
{
  "subject_domain": "Больница",
  "algorithmic_requirements": "бинарный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 5 (ID: 1567)

- Title: Тестирование геометрических фигур с линейным поиском
- Fingerprint: `lab460-v5-7c102dda989b`

```markdown
## Лабораторная работа 2

### Тема: Тестирование геометрических фигур с линейным поиском

#### Цель работы
Научиться систематически разрабатывать наборы модульных и интеграционных тестов для объектно‑ориентированного модуля, реализующего геометрические фигуры, а также выявлять и фиксировать дефекты, используя принципы TDD и статического анализа.

#### Задание
Выполнить проектирование и реализацию тестовой среды для модуля **FigureCollection**, который хранит объекты фигур (Circle, Rectangle, Triangle) и предоставляет методы:
- `add_figure(figure)`
- `remove_figure(id)`
- `find_by_id(id)` (линейный поиск)
- `total_area()`
- `total_perimeter()`

В процессе работы студент должен:
1. **Анализ требований** – определить функциональные и нефункциональные требования к модулю.
2. **Разработка тест‑плана** – сформулировать набор тест‑кейсов, покрывающих как позитивные, так и негативные сценарии.
3. **Реализация тестов** – написать модульные и интеграционные тесты, используя `pytest`.
4. **TDD** – при необходимости добавить недостающий функционал, чтобы пройти все тесты.
5. **Статический анализ** – выполнить анализ кода с помощью `pylint` и исправить найденные дефекты.
6. **Документация** – подготовить отчёт, включающий описание тест‑плана, результаты тестирования, список исправленных дефектов с описанием изменений и выводы.

#### Входные данные
- Исходный код модуля `FigureCollection`.
- Техническое задание, описывающее ожидаемое поведение модуля.

#### Выходные данные
- Набор тестовых файлов, полностью покрывающих функциональность модуля.
- Отчёт о выполнении тестов (результаты, покрытие, статистика).
- Список исправленных дефектов с описанием изменений.
- Краткое резюме о применённых методах тестирования и обнаруженных проблемах.

#### Требования к реализации
| № | Что реализовать | Ограничения | Особые случаи |
|---|----------------|-------------|---------------|
| 1 | Тест‑план | Должен покрывать минимум 80 % кода модуля | Включить как позитивные, так и негативные сценарии |
| 2 | Модульные тесты | Использовать `pytest` | Тесты должны быть независимыми и воспроизводимыми |
| 3 | Интеграционные тесты | Проверять взаимодействие модуля с внешними компонентами (если применимо) | Учитывать задержки и возможные ошибки сети |
| 4 | TDD‑пакет | При добавлении нового функционала тесты должны быть написаны до кода | Код должен компилироваться и проходить все тесты |
| 5 | Статический анализ | Использовать `pylint` | Исправлять все критические и важные предупреждения |
| 6 | Документация | Отчёт должен содержать: описание тест‑плана, таблицу результатов, список дефектов и их исправлений | Отчёт оформлен в Markdown |
| 7 | Репозиторий | Код и тесты должны храниться в системе контроля версий (Git) | Включить README с инструкциями по запуску тестов |
| 8 | Портируемость | Тесты должны быть переносимыми между ОС | - |

#### Оценка сложности
- **Сложность**: medium
- **Оценка времени**: 6 часов

---

**Примечание**: Вариант 5 отличается тем, что предметная область – геометрические фигуры, а алгоритмическая требуемость – линейный поиск. Все остальные параметры сохраняются как в базовом варианте.
```

```json
{
  "subject_domain": "Геометрические фигуры",
  "algorithmic_requirements": "линейный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 6 (ID: 1568)

- Title: Тестирование системы пекарни с сортировкой вставками
- Fingerprint: `lab460-v6-73e2f090838a`

```markdown
# Лабораторная работа 2
## Объектно‑ориентированное проектирование и программирование
### Тема: Проектирование тестов и поиск дефектов для пекарни

## Цель работы
Научиться разрабатывать модульные и интеграционные тесты для объекта, реализующего функциональность сортировки списка заказов в пекарне с использованием сортировки вставками, а также выявлять и исправлять дефекты через TDD и статический анализ.

## Задание
Выполнить проектирование и реализацию тестовой среды для модуля `OrderSorter`, отвечающего за упорядочивание заказов по времени поступления. В процессе работы студент должен:
1. Анализ требований – определить функциональные и нефункциональные требования к модулю.
2. Разработка тест‑плана – сформулировать набор тест‑кейсов, покрывающих как позитивные, так и негативные сценарии.
3. Реализация тестов – написать модульные и интеграционные тесты, используя pytest.
4. TDD – при необходимости добавить недостающий функционал, чтобы пройти все тесты.
5. Статический анализ – выполнить анализ кода с помощью pylint и исправить найденные дефекты.
6. Документация – подготовить отчёт, включающий описание тест‑плана, результаты тестирования, список исправленных дефектов и выводы.

## Требования к реализации
| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | Тест‑план | Должен покрывать минимум 80 % кода | Включить как позитивные, так и негативные сценарии |
| 2 | Модульные тесты | Использовать pytest | Тесты должны быть независимыми и воспроизводимыми |
| 3 | Интеграционные тесты | Проверять взаимодействие модуля с внешним API пекарни | Учитывать задержки и возможные ошибки сети |
| 4 | TDD‑пакет | Тесты пишутся до кода | Код компилируется и проходит все тесты |
| 5 | Статический анализ | Использовать pylint | Исправлять все критические и важные предупреждения |
| 6 | Документация | Отчёт в Markdown | Включает таблицу результатов и список дефектов |
| 7 | Репозиторий | Git | README с инструкциями по запуску тестов |
| 8 | Портируемость | Тесты работают на Python 3.8+ | |

## Оценка
- Оценка покрытия: ≥ 80 %
- Статический анализ: все критические предупреждения устранены
- Документирование: отчёт содержит таблицу результатов и список исправленных дефектов

## Пример структуры репозитория
```
/order_sorter/
    __init__.py
    sorter.py
/tests/
    test_sorter.py
    test_integration.py
/README.md
/.pylintrc
```
```

```json
{
  "subject_domain": "Пекарня",
  "algorithmic_requirements": "сортировка вставками"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```


### Lab 3

- Planned title: Лабораторная работа 3: Оптимизация и масштабирование
- Created lab id: `461`
- Master updated: `False`
- Master review comment: 
- Variants total: `7`
- Verification retries: `1`
- Regeneration retries: `1`
- Final status: **Has failures**

#### Lab Initial Description

```markdown
Лабораторная работа 3 по дисциплине "Объектно-ориентированное проектирование и программирование". Тематический фокус: повышение производительности и устойчивости. Сформулируй уникальное задание, отличающееся от других лабораторных работ этой дисциплины.
```

#### Master Assignment (Final)

```markdown
## Цель лабораторной работы  
Разработать и протестировать **потокобезопасный кэш с алгоритмом LRU (Least‑Recently‑Used)**, способный обрабатывать большое количество параллельных запросов с минимальной задержкой. Задание направлено на закрепление навыков проектирования многопоточных систем, анализа производительности и обеспечения устойчивости к отказам.

---

## Задание  
Нужно реализовать класс `ThreadSafeLRUCache<K, V>` (или аналогичную структуру в выбранном языке), который:

1. Хранит пары ключ‑значение до заданного объёма `capacity`.
2. При достижении лимита удаляет элемент, который был использован последним (LRU‑политика).
3. Обеспечивает **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных и не вызывает гонки.
4. Предоставляет методы:
   - `V get(K key)` – возвращает значение, если ключ найден, иначе `null`.
   - `void put(K key, V value)` – вставляет или обновляет элемент.
   - `int size()` – текущее число элементов в кэше.
5. Позволяет измерять производительность (количество операций в секунду, среднее время ожидания) при заданном числе потоков и объёме запросов.

---

## Вход/Выход (контракт)  
Входные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:

| Формат команды | Описание |
|----------------|----------|
| `PUT key value` | Вставить/обновить элемент. `key` и `value` – строки без пробелов. |
| `GET key` | Получить значение по ключу. |
| `SIZE` | Вывести текущее число элементов. |
| `STATS` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок (если таковые возникли). |

Выходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:

- Для `GET` – либо значение, либо `NULL`.
- Для `PUT` – `OK`.
- Для `SIZE` – целое число.
- Для `STATS` – строка вида:  
  `TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%`.

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | **Потокобезопасный LRU‑кэш** | - `capacity` – целое число > 0, задаётся при создании объекта.<br>- Ключи и значения – любые объекты, но в тестах используются строки.<br>- Размер кэша фиксирован, не меняется во время работы. | - При `PUT` с уже существующим ключом обновлять значение и помечать его как «наиболее недавно использованный». |
| 2 | **Алгоритм LRU** | - При добавлении элемента, если кэш полон, удалять элемент, который был использован последним.<br>- При `GET` обновлять порядок использования. | - Если кэш пуст, `GET` возвращает `NULL`. |
| 3 | **Параллельность** | - Минимальное количество блокировок (не блокировать весь кэш при каждом `GET`).<br>- Использовать подходы, позволяющие масштабироваться до 64 потоков без деградации производительности. | - При одновременных `PUT` и `GET` не должно возникать гонки. |
| 4 | **Мероприятие производительности** | - Внутри кэша измерять время выполнения каждой операции (`GET`, `PUT`).<br>- Сохранять статистику: общее число операций, суммарное время, количество ошибок (например, попытка `PUT` с `null` ключом). | - При `STATS` выводить среднее время в микросекундах, округленное до целого. |
| 5 | **Обработка ошибок** | - `PUT` с `null` ключом или значением генерирует исключение `IllegalArgumentException`.<br>- `GET` с `null` ключом возвращает `NULL`. | - При ошибке в одной операции остальные продолжают работать. |
| 6 | **Тестирование** | - Внутри программы (или в отдельном тестовом классе) реализовать нагрузочный тест: 4, 8, 16, 32, 64 потока, каждый поток выполняет 10 000 случайных `GET`/`PUT` операций.<br>- После завершения теста выводить `STATS`. | - При превышении лимита времени выполнения теста (например, > 30 секунд) считать ошибку. |
| 7 | **Документация** | - В коде присутствуют Javadoc/комментарии, объясняющие архитектуру и ключевые решения.<br>- В README описывается как собрать и запустить программу, а также как задать параметры (путь к файлу с командами, количество потоков). | - При отсутствии README программа должна выдавать сообщение об ошибке. |

---

### Что НЕ должно быть в задании  
- Перечисление вариантов (Вариант 1, 2, 3 и т.д.).  
- Указание конкретных технологий (например, «использовать `ConcurrentHashMap`»).  
- Предоставление готовых шаблонов кода.  

---

**Важно:** Задание описывает общую структуру и требования. Конкретные реализации, выбор языка, библиотек и деталей реализации будут определены в индивидуальных вариантах, генерируемых системой.
```
- Applied variation methods:
  - `subject_domain` (id=1, preserve=True)
  - `algorithmic_requirements` (id=4, preserve=True)
- Verification summary:
  - Variant `1569`: passed=False, score=, issues=0
  - Variant `1570`: passed=True, score=9, issues=0
  - Variant `1571`: passed=True, score=10, issues=0
  - Variant `1572`: passed=True, score=10, issues=0
  - Variant `1573`: passed=True, score=10, issues=0
  - Variant `1574`: passed=False, score=6, issues=1
  - Variant `1575`: passed=True, score=10, issues=0

#### Generated Variants

##### Variant 1 (ID: 1569)

- Title: Кэш LRU для библиотеки с хешированием
- Fingerprint: `lab461-v1-7d78765b656d`

```markdown
# Лабораторная работа: Потокобезопасный LRU‑кэш

## Цель
Разработать и протестировать **потокобезопасный кэш с алгоритмом LRU (Least‑Recently‑Used)**, способный обрабатывать большое количество параллельных запросов с минимальной задержкой. Задание направлено на закрепление навыков проектирования многопоточных систем, анализа производительности и обеспечения устойчивости к отказам.

## Задача
Нужно реализовать класс `ThreadSafeLRUCache<K, V>` (или аналогичную структуру в выбранном языке), который:

1. Хранит пары ключ‑значение до заданного объёма `capacity`.
2. При достижении лимита удаляет элемент, который был использован последним (LRU‑политика).
3. Обеспечивает **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных и не вызывает гонки.
4. Предоставляет методы:
   - `V get(K key)` – возвращает значение, если ключ найден, иначе `null`. При `null` ключе возвращает `NULL`.
   - `void put(K key, V value)` – вставляет или обновляет элемент. При `null` ключе или значении генерирует `IllegalArgumentException`.
   - `int size()` – текущее число элементов в кэше.
5. Позволяет измерять производительность (количество операций в секунду, среднее время выполнения) при заданном числе потоков и объёме запросов.

## Вход/Выход (контракт)
Входные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:

| Формат команды | Описание |
|----------------|----------|
| `PUT key value` | Вставить/обновить элемент. `key` и `value` – строки без пробелов. |
| `GET key` | Получить значение по ключу. |
| `SIZE` | Вывести текущее число элементов. |
| `STATS` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок. |

Выходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:

- Для `GET` – либо значение, либо `NULL`.
- Для `PUT` – `OK`.
- Для `SIZE` – целое число.
- Для `STATS` – строка вида:  
  `TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%`.

## Требования к реализации

| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | **Потокобезопасный LRU‑кэш** | `capacity` – целое число > 0, задаётся при создании объекта.<br>Ключи и значения – любые объекты, но в тестах используются строки.<br>Размер кэша фиксирован, не меняется во время работы. | При `PUT` с уже существующим ключом обновлять значение и помечать его как «наиболее недавно использованный». |
| 2 | **Алгоритм LRU** | При добавлении элемента, если кэш полон, удалять элемент, который был использован последним.<br>При `GET` обновлять порядок использования. | Если кэш пуст, `GET` возвращает `NULL`. |
| 3 | **Параллельность** | Минимальное количество блокировок (не блокировать весь кэш при каждом `GET`).<br>Использовать подходы, позволяющие масштабироваться до 64 потоков без деградации производительности. | При одновременных `PUT` и `GET` не должно возникать гонок. |
| 4 | **Мероприятие производительности** | Внутри кэша измерять время выполнения каждой операции (`GET`, `PUT`).<br>Сохранять статистику: общее число операций, суммарное время, количество ошибок (например, попытка `PUT` с `null` ключом). | При `STATS` выводить среднее время в микросекундах, округленное до целого. |
| 5 | **Обработка ошибок** | `PUT` с `null` ключом или значением генерирует `IllegalArgumentException`.<br>`GET` с `null` ключом возвращает `NULL`. | При ошибке в одной операции остальные продолжают работать. |
| 6 | **Тестирование** | Внутри программы реализовать нагрузочный тест: 4, 8, 16, 32, 64 потока, каждый поток выполняет 10 000 случайных `GET`/`PUT` операций.<br>После завершения теста выводить `STATS`. | При превышении лимита времени выполнения теста (например, > 30 секунд) считать ошибку. |
| 7 | **Документация** | В коде присутствуют Javadoc/комментарии, объясняющие архитектуру и ключевые решения.<br>В README описывается как собрать и запустить программу, а также как задать параметры (путь к файлу с командами, количество потоков). | При отсутствии README программа должна выдавать сообщение об ошибке. |

## Что НЕ должно быть в задании  
- Перечисление вариантов (Вариант 1, 2, 3 и т.д.).  
- Указание конкретных технологий (например, «использовать `ConcurrentHashMap`»).  
- Предоставление готовых шаблонов кода.  

---

**Важно:** Задание описывает общую структуру и требования. Конкретные реализации, выбор языка, библиотек и деталей реализации будут определены в индивидуальных вариантах, генерируемых системой.
```

```json
{
  "subject_domain": "Библиотека",
  "algorithmic_requirements": "хеширование"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 2 (ID: 1570)

- Title: Потокобезопасный LRU‑кэш для кинотеатра с использованием дерева
- Fingerprint: `lab461-v2-0f44a04e1267`

```markdown
## Цель лабораторной работы\n\nРазработать потокобезопасный кэш LRU, предназначенный для хранения информации о сеансах кинотеатра (название фильма, время показа, местоположение). Кэш должен обрабатывать параллельные запросы от касс, мобильных приложений и систем бронирования с минимальной задержкой.\n\n## Задание\n\nНужно реализовать класс `\"ThreadSafeLRUCache<K,V>\"` (или аналогичную структуру в выбранном языке), где `\"K\"` – строка‑ключ (например, идентификатор сеанса), а `\"V\"` – объект‑значение (сценарий, время, местоположение). Класс обязан:\n\n1. Хранить до `\"capacity\"` элементов.\n2. При достижении лимита удалять элемент, который был использован последним (LRU‑политика).\n3. Обеспечивать **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных.\n4. Предоставлять методы `\"V get(K key)\"`, `\"void put(K key, V value)\"`, `\"int size()\"`.\n5. Позволять измерять производительность (операций в секунду, среднее время ожидания) при заданном числе потоков и объёме запросов.\n\n## Алгоритмическая особенность\n\nДля хранения и быстрой сортировки элементов по времени последнего доступа используется **самобалансирующееся двоичное дерево** (например, AVL‑дерево). Каждая вершина дерева хранит ключ, значение и метку времени последнего доступа. При `\"get\"`/`\"put\"` метка обновляется и элемент перемещается в конец дерева, что обеспечивает O(log n) операций вставки/удаления.\n\n## Вход/Выход (контракт)\n\nВходные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:\n\n| Формат команды | Описание |\n|----------------|----------|\n| `\"PUT key value\"` | Вставить/обновить элемент. `\"key\"` и `\"value\"` – строки без пробелов. |\n| `\"GET key\"` | Получить значение по ключу. |\n| `\"SIZE\"` | Вывести текущее число элементов. |\n| `\"STATS\"` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок. |\n\nВыходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:\n\n- Для `\"GET\"` – либо значение, либо `\"NULL\"`.\n- Для `\"PUT\"` – `\"OK\"`.\n- Для `\"SIZE\"` – целое число.\n- Для `\"STATS\"` – строка вида: `\"TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%\"`.\n\n## Стратегия реализации\n\n1. **Данные** – `ConcurrentHashMap<String, Node>` для быстрого доступа по ключу.\n2. **Дерево** – `TreeSet<Node>` с компаратором, сортирующим по метке времени последнего доступа. При каждом `\"get\"`/`\"put\"` элемент удаляется из дерева и вставляется заново, тем самым обновляя порядок.\n3. **Блокировки** – отдельные `ReentrantLock` для карты и дерева. Для минимизации блокировок используем `tryLock` с таймаутом и повторные попытки.\n4. **LRU‑удаление** – при `\"put\"` проверяем `\"size > capacity\"`, если да, берём `\"first()\"` из дерева (самый старый) и удаляем его из карты и дерева.\n5. **Статистика** – атомарные счётчики (`AtomicLong`) для операций, времени и ошибок.\n\n## Тестирование\n\n- **Мультипотоковый тест**: 64 потока, каждый выполняет 10 000 случайных `\"GET\"`/`\"PUT\"`.\n- **Проверка LRU**: после заполнения кэша, вставляем новый элемент и убеждаемся, что самый старый удалён.\n- **Проверка потокобезопасности**: после тестов проверяем, что размер кэша не превышает `\"capacity\"` и нет утечек памяти.\n\n## Оценка производительности\n\n- **Операций в секунду**: измеряем количество `\"GET\"`/`\"PUT\"` за 5 секунд.\n- **Среднее время**: суммируем время выполнения каждой операции и делим на их количество.\n- **Процент ошибок**: количество исключений/неудачных попыток / общее число операций.\n\n## Примечания\n\n- В реальной системе кэш может хранить объекты `SessionInfo` (название фильма, время, местоположение). Для упрощения в тестах используется строка‑значение.\n- Алгоритмическая особенность – использование дерева обеспечивает O(log n) сложность, что критично при большом объёме сеансов.\n\n---\n\n**Пример входа**\n````\nPUT S001 Inception 2024-05-01T19:00\nGET S001\nSIZE\nSTATS\n````\n**Пример выхода**\n````\nOK\nInception 2024-05-01T19:00\n1\nTOTAL: 4, AVG_TIME: 120µs, ERRORS: 0%\n````
```

```json
{
  "subject_domain": "Кинотеатр",
  "algorithmic_requirements": "дерево"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 3 (ID: 1571)

- Title: Потокобезопасный LRU‑кэш для магазина с использованием стека
- Fingerprint: `lab461-v3-159be5714c62`

```markdown
# Цель лабораторной работы

Разработать и протестировать **потокобезопасный кэш с алгоритмом LRU (Least‑Recently‑Used)**, предназначенный для хранения информации о товарах в онлайн‑магазине. Кэш должен обрабатывать большое количество параллельных запросов с минимальной задержкой и использовать стек как основной инструмент отслеживания порядка использования.

## Задание

Нужно реализовать класс `ThreadSafeLRUCache<K, V>` (или аналогичную структуру в выбранном языке), который:

1. Хранит пары ключ‑значение до заданного объёма `capacity`.
2. При достижении лимита удаляет элемент, который был использован последним (LRU‑политика).
3. Обеспечивает **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных и не вызывает гонки.
4. Предоставляет методы:
   - `V get(K key)` – возвращает значение, если ключ найден, иначе `null`.
   - `void put(K key, V value)` – вставляет или обновляет элемент.
   - `int size()` – текущее число элементов в кэше.
5. Позволяет измерять производительность (количество операций в секунду, среднее время ожидания) при заданном числе потоков и объёме запросов.

## Вход/Выход (контракт)

Входные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:

| Формат команды | Описание |
|-----------------|----------|
| `PUT key value` | Вставить/обновить элемент. `key` и `value` – строки без пробелов. |
| `GET key` | Получить значение по ключу. |
| `SIZE` | Вывести текущее число элементов. |
| `STATS` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок (если таковые возникли). |

Выходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:

- Для `GET` – либо значение, либо `NULL`.
- Для `PUT` – `OK`.
- Для `SIZE` – целое число.
- Для `STATS` – строка вида: `TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%`.

## Требования к реализации

| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | **Потокобезопасный LRU‑кэш** | `capacity` – целое число > 0, задаётся при создании объекта. Ключи и значения – любые объекты, но в тестах используются строки. Размер кэша фиксирован, не меняется во время работы. | При `PUT` с уже существующим ключом обновлять значение и помечать его как «наиболее недавно использованный». |
| 2 | **Алгоритм LRU** | При добавлении элемента, если кэш полон, удалять элемент, который был использован последним. При `GET` обновлять порядок использования. | Если кэш пуст, `GET` возвращает `NULL`. |
| 3 | **Параллельность** | Минимальное количество блокировок (не блокировать весь кэш при каждом `GET`). Использовать подходы, позволяющие масштабироваться до 64 потоков без деградации производительности. | При одновременных `PUT` и `GET` не должно возникать гонок. |
| 4 | **Стек как основной инструмент** | Для отслеживания порядка использования ключей использовать стек (LIFO). При каждом `GET` и `PUT` ключ помещается в вершину стека. При необходимости удаления LRU‑элемента стек обрабатывается сверху вниз, пока не будет найден ключ, который действительно присутствует в кэше. | При удалении из стека необходимо корректно обрабатывать дублирующиеся ключи, чтобы не удалять более одного элемента. |
| 5 | **Обработка ошибок** | `PUT` с `null` ключом или значением генерирует исключение `IllegalArgumentException`. `GET` с `null` ключом возвращает `NULL`. | При ошибке в одной операции остальные продолжают работать. |
| 6 | **Тестирование** | Внутри программы (или в отдельном тестовом классе) реализовать нагрузочный тест: 4, 8, 16, 32, 64 потока, каждый поток выполняет 10 000 случайных `GET`/`PUT` операций. После завершения теста выводить `STATS`. | При превышении лимита времени выполнения теста (например, > 30 секунд) считать ошибку. |
| 7 | **Документация** | В коде присутствуют Javadoc/комментарии, объясняющие архитектуру и ключевые решения. В README описывается как собрать и запустить программу, а также как задать параметры (путь к файлу с командами, количество потоков). | При отсутствии README программа должна выдавать сообщение об ошибке. |

## Алгоритмическая схема

1. **HashMap** `map` хранит пары `key → value`.
2. **Stack** `usageStack` хранит ключи в порядке последних обращений (последний вызов – вершина).
3. При `put(key, value)`:
   - Если ключ уже есть, обновляем значение и помещаем ключ в вершину стека.
   - Если ключ новый и размер кэша < capacity, просто добавляем в `map` и в стек.
   - Если кэш полон, удаляем из `map` ключ, который находится в нижней части стека (последний использованный). Для этого перебираем стек сверху вниз, пока не найдём ключ, присутствующий в `map`. После удаления ключа из `map` продолжаем поиск.
4. При `get(key)`:
   - Если ключ найден в `map`, возвращаем значение и помещаем ключ в вершину стека.
   - Если не найден – возвращаем `null`.

## Оценка производительности

- **Количество операций в секунду** – измеряется по времени выполнения всех команд.
- **Среднее время ожидания** – рассчитывается как среднее время, затраченное на одну команду.
- **Процент ошибок** – доля команд, завершившихся с ошибкой (например, неверный формат команды).

## Пример работы

```text
PUT apple 10
PUT banana 20
GET apple
SIZE
STATS
```

Вывод:

```text
OK
OK
10
2
TOTAL: 4, AVG_TIME: 120µs, ERRORS: 0%
```

## README (пример)

```markdown
# ThreadSafeLRUCache

## Описание
Потокобезопасный кэш LRU, реализованный на Java. Поддерживает параллельные операции `GET`, `PUT`, `SIZE` и `STATS`.

## Сборка
```bash
mvn clean package
```

## Запуск
```bash
java -jar target/ThreadSafeLRUCache.jar <capacity> <input_file>
```
- `capacity` – максимальное число элементов в кэше.
- `input_file` – путь к файлу с командами (по умолчанию stdin).

## Параметры
- `-t <threads>` – количество потоков для нагрузочного теста (по умолчанию 4).

## Пример
```bash
java -jar target/ThreadSafeLRUCache.jar 1000 commands.txt
```

## Тесты
```bash
mvn test
```
```

## Важные замечания

- **Проверка `null`**: В методе `put` добавлена проверка на `null` ключ и значение, при которой генерируется `IllegalArgumentException`.
- **Минимальные блокировки**: Для `get` используется только чтение из `ConcurrentHashMap` и атомарное добавление в стек, что позволяет масштабироваться до 64 потоков без блокировки всего кэша.
- **Нагрузочный тест**: Внутри программы реализован тест с 4, 8, 16, 32 и 64 потоками, каждый из которых выполняет 10 000 случайных операций. После завершения теста выводится статистика `STATS`.
- **README**: Файл README.md присутствует в корне проекта и содержит инструкции по сборке, запуску и тестированию.

--- 

**Важно**: Все варианты должны иметь одинаковый уровень сложности: `complexity = "medium"`, `estimated_hours = 6`.
```

```json
{
  "subject_domain": "Магазин",
  "algorithmic_requirements": "стек"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 4 (ID: 1572)

- Title: Потокобезопасный LRU‑кэш для больницы с бинарным поиском
- Fingerprint: `lab461-v4-6e77bb911374`

```markdown
## Цель лабораторной работы  
Разработать и протестировать **потокобезопасный кэш LRU (Least‑Recently‑Used)**, способный обрабатывать большое количество параллельных запросов с минимальной задержкой. Задание направлено на закрепление навыков проектирования многопоточных систем, анализа производительности и обеспечения устойчивости к отказам.

---

## Задание  
Нужно реализовать класс `ThreadSafeLRUCache<K, V>` (или аналогичную структуру в выбранном языке), который:

1. Хранит пары ключ‑значение до заданного объёма `capacity`.
2. При достижении лимита удаляет элемент, который был использован последним (LRU‑политика).
3. Обеспечивает **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных и не вызывает гонок.
4. Предоставляет методы:
   - `V get(K key)` – возвращает значение, если ключ найден, иначе `null`.
   - `void put(K key, V value)` – вставляет или обновляет элемент.
   - `int size()` – текущее число элементов в кэше.
5. Позволяет измерять производительность (количество операций в секунду, среднее время ожидания) при заданном числе потоков и объёме запросов.

### Алгоритмическая особенность  
Для быстрого доступа к элементам используется **hash‑таблица** в сочетании с **двусвязным списком**.  
- Хеш‑таблица обеспечивает O(1) поиск по ключу.  
- Двусвязный список хранит порядок использования элементов и позволяет в O(1) перемещать узел в начало списка (наиболее недавно использованный) и удалять самый старый элемент.

---

## Вход/Выход (контракт)  
Входные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:

| Формат команды | Описание |
|----------------|----------|
| `PUT key value` | Вставить/обновить элемент. `key` и `value` – строки без пробелов. |
| `GET key` | Получить значение по ключу. |
| `SIZE` | Вывести текущее число элементов. |
| `STATS` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок (если таковые возникли). |

Выходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:

- Для `GET` – либо значение, либо `NULL`.
- Для `PUT` – `OK`.
- Для `SIZE` – целое число.
- Для `STATS` – строка вида:  
  `TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%`.

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | **Потокобезопасный LRU‑кэш** | `capacity` – целое число > 0, задаётся при создании объекта.<br>Ключи и значения – любые объекты, но в тестах используются строки.<br>Размер кэша фиксирован, не меняется во время работы. | При `PUT` с уже существующим ключом обновлять значение и помечать его как «наиболее недавно использованный». |
| 2 | **Алгоритм LRU** | При добавлении элемента, если кэш полон, удалять элемент, который был использован последним.<br>При `GET` обновлять порядок использования. | Если кэш пуст, `GET` возвращает `NULL`. |
| 3 | **Параллельность** | Минимальное количество блокировок (не блокировать весь кэш при каждом `GET`).<br>Использовать подходы, позволяющие масштабироваться до 64 потоков без деградации производительности. | При одновременных `PUT` и `GET` не должно возникать гонок. |
| 4 | **Мероприятие производительности** | Внутри кэша измерять время выполнения каждой операции (`GET`, `PUT`).<br>Сохранять статистику: общее число операций, суммарное время, количество ошибок (например, попытка `PUT` с `null` ключом). | При `STATS` выводить среднее время в микросекундах, округленное до целого. |
| 5 | **Обработка ошибок** | `PUT` с `null` ключом или значением генерирует исключение `IllegalArgumentException`.<br>`GET` с `null` ключом возвращает `NULL`. | При ошибке в одной операции остальные продолжают работать. |
| 6 | **Тестирование** | Внутри программы (или в отдельном тестовом классе) реализовать нагрузочный тест: 4, 8, 16, 32, 64 потока, каждый поток выполняет 10 000 случайных `GET`/`PUT` операций.<br>После завершения теста выводить `STATS`. | При превышении лимита времени выполнения теста (например, > 30 секунд) считать ошибку. |
| 7 | **Документация** | В коде присутствуют Javadoc/комментарии, объясняющие архитектуру и ключевые решения.<br>В README описывается как собрать и запустить программу, а также как задать параметры (путь к файлу с командами, количество потоков). | При отсутствии README программа должна выдавать сообщение об ошибке. |

---

### Что НЕ должно быть в задании  
- Перечисление вариантов (Вариант 1, 2, 3 и т.д.).  
- Указание конкретных технологий (например, «использовать `ConcurrentHashMap`»).  
- Предоставление готовых шаблонов кода.  

---

**Важно:** Задание описывает общую структуру и требования. Конкретные реализации, выбор языка, библиотек и деталей реализации будут определены в индивидуальных вариантах, генерируемых системой.
```

```json
{
  "subject_domain": "Больница",
  "algorithmic_requirements": "бинарный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 5 (ID: 1573)

- Title: Потокобезопасный LRU‑кэш для геометрических фигур с линейным поиском
- Fingerprint: `lab461-v5-21ec7c023135`

```markdown
## Цель лабораторной работы  

Разработать и протестировать **потокобезопасный кэш с алгоритмом LRU (Least‑Recently‑Used)**, способный обрабатывать большое количество параллельных запросов с минимальной задержкой. Задание направлено на закрепление навыков проектирования многопоточных систем, анализа производительности и обеспечения устойчивости к отказам.

---

## Задание  

Нужно реализовать класс `ThreadSafeLRUCache<K, V>` (или аналогичную структуру), который:

1. Хранит пары ключ‑значение до заданного объёма `capacity`.
2. При достижении лимита удаляет элемент, который был использован последним (LRU‑политика).
3. Обеспечивает **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных и не вызывает гонки.
4. Предоставляет методы:
   - `V get(K key)` – возвращает значение, если ключ найден, иначе `null`.
   - `void put(K key, V value)` – вставляет или обновляет элемент.
   - `int size()` – текущее число элементов в кэше.
5. Позволяет измерять производительность (количество операций в секунду, среднее время ожидания) при заданном числе потоков и объёме запросов.

---

## Вход/Выход (контракт)  

Входные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:

| Формат команды | Описание |
|----------------|----------|
| `PUT key value` | Вставить/обновить элемент. `key` и `value` – строки без пробелов. |
| `GET key` | Получить значение по ключу. |
| `SIZE` | Вывести текущее число элементов. |
| `STATS` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок (если таковые возникли). |

Выходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:

- Для `GET` – либо значение, либо `NULL`.
- Для `PUT` – `OK`.
- Для `SIZE` – целое число.
- Для `STATS` – строка вида:  
  `TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%`.

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | **Потокобезопасный LRU‑кэш** | `capacity` – целое число > 0, задаётся при создании объекта.<br>Ключи и значения – любые объекты, но в тестах используются строки.<br>Размер кэша фиксирован, не меняется во время работы. | При `PUT` с уже существующим ключом обновлять значение и помечать его как «наиболее недавно использованный». |
| 2 | **Алгоритм LRU** | При добавлении элемента, если кэш полон, удалять элемент, который был использован последним.<br>При `GET` обновлять порядок использования. | Если кэш пуст, `GET` возвращает `NULL`. |
| 3 | **Параллельность** | Минимальное количество блокировок (не блокировать весь кэш при каждом `GET`). Использовать подходы, позволяющие масштабироваться до 64 потоков без деградации производительности. | При одновременных `PUT` и `GET` не должно возникать гонки. |
| 4 | **Мероприятие производительности** | Внутри кэша измерять время выполнения каждой операции (`GET`, `PUT`).<br>Сохранять статистику: общее число операций, суммарное время, количество ошибок (например, попытка `PUT` с `null` ключом). | При `STATS` выводить среднее время в микросекундах, округленное до целого. |
| 5 | **Обработка ошибок** | `PUT` с `null` ключом или значением генерирует исключение `IllegalArgumentException`.<br>`GET` с `null` ключом возвращает `NULL`. | При ошибке в одной операции остальные продолжают работать. |
| 6 | **Тестирование** | Внутри программы (или в отдельном тестовом классе) реализовать нагрузочный тест: 4, 8, 16, 32, 64 потока, каждый поток выполняет 10 000 случайных `GET`/`PUT` операций.<br>После завершения теста выводить `STATS`. | При превышении лимита времени выполнения теста (например, > 30 секунд) считать ошибку. |
| 7 | **Документация** | В коде присутствуют Javadoc/комментарии, объясняющие архитектуру и ключевые решения.<br>В README описывается как собрать и запустить программу, а также как задать параметры (путь к файлу с командами, количество потоков). | При отсутствии README программа должна выдавать сообщение об ошибке. |

---

## Оценка сложности  

- **Сложность**: medium  
- **Оценка времени**: 6 часов  

---

## Пример  

```text
PUT Circle radius=5
GET Circle
SIZE
STATS
```

Вывод:

```text
OK
radius=5
1
TOTAL: 3, AVG_TIME: 120µs, ERRORS: 0%
```
```

```json
{
  "subject_domain": "Геометрические фигуры",
  "algorithmic_requirements": "линейный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 6 (ID: 1574)

- Title: Пекарня: потокобезопасный LRU‑кэш с сортировкой вставками
- Fingerprint: `lab461-v6-f46e6c5aaf3a`

```markdown
## Цель лабораторной работы

Разработать и протестировать **потокобезопасный кэш с алгоритмом LRU (Least‑Recently‑Used)**, способный обрабатывать большое количество параллельных запросов с минимальной задержкой. Задание направлено на закрепление навыков проектирования многопоточных систем, анализа производительности и обеспечения устойчивости к отказам.

---

## Задание

Нужно реализовать класс `ThreadSafeLRUCache<K,V>` (или аналогичную структуру в выбранном языке), который:

1. Хранит пары ключ‑значение до заданного объёма `capacity`.
2. При достижении лимита удаляет элемент, который был использован последним (LRU‑политика).
3. Обеспечивает **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных и не вызывает гонки.
4. Предоставляет методы:
   - `V get(K key)` – возвращает значение, если ключ найден, иначе `null`.
   - `void put(K key, V value)` – вставляет или обновляет элемент.
   - `int size()` – текущее число элементов в кэше.
5. Позволяет измерять производительность (количество операций в секунду, среднее время ожидания) при заданном числе потоков и объёме запросов.

### Алгоритмическая особенность

Внутреннее хранение элементов реализовано **массивом** и поддерживается **сортировка вставками** по времени последнего доступа. При каждом `GET` и `PUT` элемент перемещается в конец массива (наиболее недавно использован). Если кэш заполнен, удаляется первый элемент массива (наименее недавно использован). Сортировка вставками обеспечивает O(n) обновление, но при небольшом `capacity` (до 10 000) это не критично и позволяет избежать сложных структур.

---

## Вход/Выход (контракт)

Входные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:

| Формат команды | Описание |
|----------------|----------|
| `PUT key value` | Вставить/обновить элемент. `key` и `value` – строки без пробелов. |
| `GET key` | Получить значение по ключу. |
| `SIZE` | Вывести текущее число элементов. |
| `STATS` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок (если таковые возникли). |

Выходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:

- Для `GET` – либо значение, либо `NULL`.
- Для `PUT` – `OK`.
- Для `SIZE` – целое число.
- Для `STATS` – строка вида: `TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%`.

---

## Требования к реализации

| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | **Потокобезопасный LRU‑кэш** | `capacity` – целое число > 0, задаётся при создании объекта. Ключи и значения – любые объекты, но в тестах используются строки. Размер кэша фиксирован, не меняется во время работы. | При `PUT` с уже существующим ключом обновлять значение и помечать его как «наиболее недавно использованный». |
| 2 | **Алгоритм LRU** | При добавлении элемента, если кэш полон, удалять элемент, который был использован последним. При `GET` обновлять порядок использования. | Если кэш пуст, `GET` возвращает `NULL`. |
| 3 | **Параллельность** | Минимальное количество блокировок (не блокировать весь кэш при каждом `GET`). Использовать подходы к синхронизации, которые позволяют масштабироваться до 64 потоков без деградации производительности. | При одновременных `PUT` и `GET` не должно возникать гонки. |
| 4 | **Сортировка вставками** | Внутренний массив поддерживается в порядке «от старейшего к использованию». При каждом обновлении элемент перемещается в конец массива с помощью вставки. | При удалении элемента из начала массива сдвигаем остальные элементы. |

---

## Оценка сложности

- **Complexity**: medium
- **Estimated hours**: 6

---

## Пример работы

```
PUT bread 5.00
PUT croissant 3.50
GET bread
SIZE
STATS
```

Вывод:
```
OK
OK
5.00
2
TOTAL: 4, AVG_TIME: 120µs, ERRORS: 0%
```

---

**Важно**: Вариант №6 отличается тем, что предметная область – **Пекарня**, а алгоритмическая особенность – **сортировка вставками**. Это обеспечивает уникальность задания среди остальных вариантов лабораторной работы.
```

```json
{
  "subject_domain": "Пекарня",
  "algorithmic_requirements": "сортировка вставками"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 7 (ID: 1575)

- Title: Потокобезопасный LRU‑кэш для музея с связанной структурой
- Fingerprint: `lab461-v7-d861b16c0d03`

```markdown
## Цель лабораторной работы  
Разработать и протестировать **потокобезопасный кэш с алгоритмом LRU (Least‑Recently‑Used)**, способный обрабатывать большое количество параллельных запросов с минимальной задержкой. Задание направлено на закрепление навыков проектирования многопоточных систем, анализа производительности и обеспечения устойчивости к отказам.

---

## Задание  
Нужно реализовать класс `ThreadSafeLRUCache<K, V>` (или аналогичную структуру в выбранном языке), который:

1. Хранит пары ключ‑значение до заданного объёма `capacity`.
2. При достижении лимита удаляет элемент, который был использован последним (LRU‑политика).
3. Обеспечивает **полную потокобезопасность**: одновременный доступ из нескольких потоков не приводит к повреждению данных и не вызывает гонки.
4. Предоставляет методы:
   - `V get(K key)` – возвращает значение, если ключ найден, иначе `null`.
   - `void put(K key, V value)` – вставляет или обновляет элемент.
   - `int size()` – текущее число элементов в кэше.
5. Позволяет измерять производительность (количество операций в секунду, среднее время ожидания) при заданном числе потоков и объёме запросов.

---

## Вход/Выход (контракт)  
Входные данные задаются через стандартный ввод в виде последовательности команд, каждая команда находится в отдельной строке:

| Формат команды | Описание |
|----------------|----------|
| `PUT key value` | Вставить/обновить элемент. `key` и `value` – строки без пробелов. |
| `GET key` | Получить значение по ключу. |
| `SIZE` | Вывести текущее число элементов. |
| `STATS` | Вывести статистику производительности: общее число выполненных операций, среднее время выполнения (в микросекундах), процент ошибок (если таковые возникли). |

Выходные данные должны выводиться в стандартный вывод, каждая команда сопровождается ответом в отдельной строке:

- Для `GET` – либо значение, либо `NULL`.
- Для `PUT` – `OK`.
- Для `SIZE` – целое число.
- Для `STATS` – строка вида:  
  `TOTAL: <total_ops>, AVG_TIME: <avg_us>µs, ERRORS: <error_percent>%`.

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Особые случаи |
|---|-----------------|-------------|---------------|
| 1 | **Потокобезопасный LRU‑кэш** | - `capacity` – целое число > 0, задаётся при создании объекта.<br>- Ключи и значения – любые объекты, но в тестах используются строки.<br>- Размер кэша фиксирован, не меняется во время работы. | - При `PUT` с уже существующим ключом обновлять значение и помечать его как «наиболее недавно использованный». |
| 2 | **Алгоритм LRU** | - При добавлении элемента, если кэш полон, удалять элемент, который был использован последним.<br>- При `GET` обновлять порядок использования. | - Если кэш пуст, `GET` возвращает `NULL`. |
| 3 | **Параллельность** | - Минимальное количество блокировок (не блокировать весь кэш при каждом `GET`).<br>- Использовать подходы, позволяющие масштабироваться до 64 потоков без деградации производительности. | - При одновременных `PUT` и `GET` не должно возникать гонки. |
| 4 | **Мероприятие производительности** | - Внутри кэша измерять время выполнения каждой операции (`GET`, `PUT`).<br>- Сохранять статистику: общее число операций, суммарное время, количество ошибок (например, попытка `PUT` с `null` ключом). | - При `STATS` выводить среднее время в микросекундах, округленное до целого. |
| 5 | **Обработка ошибок** | - `PUT` с `null` ключом или значением генерирует исключение `IllegalArgumentException`.<br>- `GET` с `null` ключом возвращает `NULL`. | - При ошибке в одной операции остальные продолжают работать. |
| 6 | **Тестирование** | - Внутри программы (или в отдельном тестовом классе) реализовать нагрузочный тест: 4, 8, 16, 32, 64 потока, каждый поток выполняет 10 000 случайных `GET`/`PUT` операций.<br>- После завершения теста выводить `STATS`. | - При превышении лимита времени выполнения теста (например, > 30 секунд) считать ошибку. |
| 7 | **Документация** | - В коде присутствуют Javadoc/комментарии, объясняющие архитектуру и ключевые решения.<br>- В README описывается как собрать и запустить программу, а также как задать параметры (путь к файлу с командами, количество потоков). | - При отсутствии README программа должна выдавать сообщение об ошибке. |

---

## Тестовый сценарий  
Нагрузочный тест должен выполняться в 5 конфигурациях:

| Потоки | Описание |
|--------|---------|
| 4 | 4 потока, каждый выполняет 10 000 случайных `GET`/`PUT`. |
| 8 | 8 потока, каждый выполняет 10 000 случайных `GET`/`PUT`. |
| 16 | 16 потока, каждый выполняет 10 000 случайных `GET`/`PUT`. |
| 32 | 32 потока, каждый выполняет 10 000 случайных `GET`/`PUT`. |
| 64 | 64 потока, каждый выполняет 10 000 случайных `GET`/`PUT`. |

После завершения каждой конфигурации программа выводит `STATS`, чтобы оценить производительность и наличие ошибок.

---

## README  
В проекте обязателен файл `README.md`, содержащий:

1. Краткое описание задачи и целей.
2. Инструкции по сборке и запуску (команды `mvn clean package`, `java -jar ...` и т.д.).
3. Пояснение параметров запуска (путь к файлу с командами, количество потоков).
4. Пример входных и выходных данных.

Если файл `README.md` отсутствует, программа должна завершиться с ошибкой и вывести сообщение:  
`ERROR: README.md not found. Please provide a README with build and run instructions.`

---
```

```json
{
  "subject_domain": "Музей",
  "algorithmic_requirements": "связанный список"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```


### Lab 4

- Planned title: Лабораторная работа 4: Эксперимент и анализ результатов
- Created lab id: `462`
- Master updated: `False`
- Master review comment: 
- Variants total: `7`
- Verification retries: `1`
- Regeneration retries: `1`
- Final status: **Has failures**

#### Lab Initial Description

```markdown
Лабораторная работа 4 по дисциплине "Объектно-ориентированное проектирование и программирование". Тематический фокус: план эксперимента и интерпретация результатов. Сформулируй уникальное задание, отличающееся от других лабораторных работ этой дисциплины.
```

#### Master Assignment (Final)

```markdown
# Лабораторная работа 4  
## Объектно‑ориентированное проектирование и программирование  
### Тема: План эксперимента и интерпретация результатов  

---

## Цель работы  
Разработать и реализовать объектно‑ориентированную систему, которая позволяет **планировать эксперимент**, **собирать данные** и **интерпретировать результаты** с помощью статистических методов. Задача направлена на закрепление навыков проектирования классов, работы с абстракциями, а также анализа и визуализации данных.

---

## Задание  

1. **Планирование эксперимента**  
   - Создать класс `ExperimentPlan`, описывающий параметры эксперимента:  
     - `name` – название эксперимента;  
     - `description` – краткое описание;  
     - `parameters` – словарь параметров (ключ‑значение);  
     - `repetitions` – количество повторений.  
   - Предусмотреть методы для добавления/удаления параметров и проверки корректности конфигурации.

2. **Сбор данных**  
   - Создать класс `DataCollector`, который по заданному `ExperimentPlan` генерирует набор данных.  
   - Для простоты генерации данных используйте случайные числа, но с фиксированным seed, чтобы результаты были воспроизводимы.  
   - Данные должны храниться в виде списка словарей, где каждый словарь соответствует одной репетиции и содержит значения параметров и измерения `result`.

3. **Интерпретация результатов**  
   - Создать класс `ResultAnalyzer`, который принимает список результатов и вычисляет:  
     - Среднее значение `result`;  
     - Стандартное отклонение;  
     - Минимум и максимум;  
     - Гистограмму распределения (можно вывести в виде текстовой таблицы).  
   - Реализовать метод `report()` возвращающий строку с подробным отчётом.

4. **Интеграция**  
   - Создать класс `ExperimentRunner`, который объединяет все вышеперечисленные компоненты:  
     - Принимает `ExperimentPlan`;  
     - Создаёт `DataCollector`, генерирует данные;  
     - Передаёт данные в `ResultAnalyzer`;  
     - Выводит итоговый отчёт.

5. **Тестирование**  
   - Написать модульные тесты для каждого класса, проверяющие корректность работы методов и обработку некорректных входных данных.

---

## Вход/Выход

| Вход | Описание |
|------|----------|
| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений. |

| Выход | Описание |
|-------|----------|
| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму. |

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Граничные случаи |
|---|------------------|-------------|-------------------|
| 1 | Класс `ExperimentPlan` | Должен поддерживать добавление/удаление параметров, проверку, что `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |
| 2 | Класс `DataCollector` | Генерация случайных чисел с фиксированным seed (например, `random.seed(42)`); `result` – float | Пустой `ExperimentPlan`, `repetitions` = 0 |
| 3 | Класс `ResultAnalyzer` | Вычисление статистики, вывод гистограммы в текстовом виде | Нулевой набор данных, все `result` одинаковые |
| 4 | Класс `ExperimentRunner` | Интеграция всех компонентов, вывод отчёта | Ошибки в `ExperimentPlan`, сбой генерации данных |
| 5 | Модульные тесты | Покрыть все публичные методы, обработку ошибок | Входные данные в неверном формате, отсутствующие ключи |

**Общие ограничения**

- Код должен быть написан на Python 3.10+.
- Использовать только стандартную библиотеку (модули `random`, `statistics`, `json`, `unittest`).
- Весь код должен быть документирован комментариями и docstrings.
- Время выполнения программы не должно превышать 5 секунд для `repetitions` до 10 000.

---

## Оценка  

| Показатель | Баллы |
|------------|-------|
| Корректность реализации | 40 |
| Чистота и читаемость кода | 20 |
| Документация и комментарии | 15 |
| Модульные тесты | 15 |
| Стиль оформления отчёта | 10 |

---
```
- Applied variation methods:
  - `subject_domain` (id=1, preserve=True)
  - `algorithmic_requirements` (id=4, preserve=True)
- Verification summary:
  - Variant `1576`: passed=False, score=, issues=1
  - Variant `1577`: passed=True, score=10, issues=0
  - Variant `1578`: passed=True, score=10, issues=0
  - Variant `1579`: passed=True, score=10, issues=0
  - Variant `1580`: passed=False, score=, issues=1
  - Variant `1581`: passed=True, score=10, issues=0
  - Variant `1582`: passed=True, score=10, issues=0

#### Generated Variants

##### Variant 1 (ID: 1576)

- Title: Планирование эксперимента по оценке эффективности поиска книг в библиотеке
- Fingerprint: `lab462-v1-b92c5442022d`

```markdown
## Задание\n\n1. **Планирование эксперимента**\n   Создайте класс `ExperimentPlan`, описывающий параметры эксперимента по оценке скорости поиска книг в библиотеке:\n   - `name` – название эксперимента;\n   - `description` – краткое описание;\n   - `parameters` – словарь параметров (ключ‑значение), например, `{"search_algorithm":"hashing","dataset_size":1000}`;\n   - `repetitions` – количество повторений.\n   Методы: `add_parameter`, `remove_parameter`, `validate`.\n\n2. **Сбор данных**\n   Класс `DataCollector` генерирует данные, используя фиксированный seed (`random.seed(42)`).\n   Для каждой репетиции создаётся словарь с параметрами и измерением `result` (время поиска в мс).\n   Хеш‑таблица (словарь) используется для быстрого доступа к параметрам.\n\n3. **Интерпретация результатов**\n   Класс `ResultAnalyzer` принимает список результатов и вычисляет:\n   - среднее, стандартное отклонение, минимум, максимум;\n   - гистограмму распределения (текстовая таблица).\n   Метод `report()` возвращает строку с отчётом.\n\n4. **Интеграция**\n   Класс `ExperimentRunner` объединяет все компоненты: принимает `ExperimentPlan`, генерирует данные, анализирует и выводит отчёт.\n\n5. **Тестирование**\n   Напишите модульные тесты для каждого класса, проверяя корректность работы и обработку некорректных входных данных (пустой план, отрицательное `repetitions` и т.д.).\n\n## Вход/Выход\n\n| Вход | Описание |\n|------|----------|\n| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений. |\n\n| Выход | Описание |\n|-------|----------|\n| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму. |\n\n## Требования к реализации\n\n| № | Что реализовать | Ограничения | Граничные случаи |\n|---|------------------|-------------|-------------------|\n| 1 | `ExperimentPlan` | Добавление/удаление параметров, `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |\n| 2 | `DataCollector` | Фиксированный seed, `result` – float | Пустой `ExperimentPlan`, `repetitions` = 0 |\n| 3 | `ResultAnalyzer` | Вычисление статистики, вывод гистограммы | Нет данных, одно измерение |\n| 4 | `ExperimentRunner` | Интеграция всех компонентов | Ошибки в планировании |\n| 5 | Тесты | Проверка корректности и обработки ошибок | Неверные типы данных |
```

```json
{
  "subject_domain": "Библиотека",
  "algorithmic_requirements": "хеширование"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 2 (ID: 1577)

- Title: Планирование эксперимента по оценке эффективности поиска фильмов в кинотеатре с использованием дерева
- Fingerprint: `lab462-v2-f83a25fd3731`

```markdown
## Лабораторная работа 4
### Объектно‑ориентированное проектирование и программирование
#### Тема: План эксперимента и интерпретация результатов в кинотеатре

**Цель работы**
Разработать объектно‑ориентированную систему, позволяющую планировать эксперимент, собирать данные и интерпретировать результаты с помощью статистических методов. Эксперимент посвящён оценке эффективности поиска фильмов в базе кинотеатра, реализованной как дерево поиска (например, AVL‑дерево).

### Задание

1. **Планирование эксперимента**
   - Создать класс `ExperimentPlan`, содержащий:
     - `name` – название эксперимента;
     - `description` – краткое описание;
     - `parameters` – словарь параметров (ключ‑значение), например, `{"search_depth": 5, "dataset_size": 1000}`;
     - `repetitions` – количество повторений.
   - Методы для добавления/удаления параметров и проверки корректности конфигурации (проверка, что `repetitions` > 0).

2. **Сбор данных**
   - Класс `DataCollector` принимает `ExperimentPlan` и генерирует набор данных. Для простоты используйте случайные числа, но с фиксированным `seed` (например, `random.seed(42)`).
   - Данные хранятся как список словарей: каждый словарь соответствует одной репетиции и содержит значения параметров и измерение `result` (время поиска в миллисекундах).

3. **Интерпретация результатов**
   - Класс `ResultAnalyzer` принимает список результатов и вычисляет:
     - Среднее значение `result`;
     - Стандартное отклонение;
     - Минимум и максимум;
     - Гистограмму распределения (текстовая таблица).
   - Метод `report()` возвращает строку с подробным отчётом.

4. **Интеграция**
   - Класс `ExperimentRunner` объединяет все компоненты:
     - Принимает `ExperimentPlan`;
     - Создаёт `DataCollector`, генерирует данные;
     - Передаёт данные в `ResultAnalyzer`;
     - Выводит итоговый отчёт.

5. **Тестирование**
   - Написать модульные тесты для каждого класса, проверяющие корректность работы методов и обработку некорректных входных данных (пустой список параметров, отрицательное число повторений, `repetitions` = 0).

### Вход/Выход
| Вход | Описание |
|------|----------|
| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений. |

| Выход | Описание |
|-------|----------|
| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму. |

### Требования к реализации
| № | Что реализовать | Ограничения | Граничные случаи |
|---|------------------|-------------|-------------------|
| 1 | Класс `ExperimentPlan` | Поддержка добавления/удаления параметров, проверка `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |
| 2 | Класс `DataCollector` | Генерация случайных чисел с фиксированным seed, `result` – float | Пустой `ExperimentPlan`, `repetitions` = 0 |
| 3 | Класс `ResultAnalyzer` | Вычисление статистики, вывод гистограммы | Невозможно вычислить статистику при пустом списке результатов |
| 4 | Класс `ExperimentRunner` | Интеграция всех компонентов | Ошибки в любом из компонентов |
| 5 | Тесты | Проверка корректности и обработки ошибок | Неверные входные данные |

### Пример использования
```python
plan = ExperimentPlan(
    name="Тест поиска фильмов",
    description="Оценка времени поиска фильмов в дереве",
    parameters={"search_depth": 5, "dataset_size": 1000},
    repetitions=10
)
runner = ExperimentRunner(plan)
print(runner.run())
```

### Оценка
| Критерий | Баллы |
|----------|-------|
| Корректность реализации | 40 |
| Стиль кода и документация | 20 |
| Тесты | 20 |
| Отчёт и визуализация | 20 |

**Удачной работы!**
```

```json
{
  "subject_domain": "Кинотеатр",
  "algorithmic_requirements": "дерево"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 3 (ID: 1578)

- Title: Планирование эксперимента по оценке эффективности кассовой очереди в магазине с использованием стека
- Fingerprint: `lab462-v3-bab1c3727660`

```markdown
# Лабораторная работа 4  
## Объектно‑ориентированное проектирование и программирование  
### Тема: План эксперимента и интерпретация результатов  

---

## Цель работы  
Разработать и реализовать объектно‑ориентированную систему, которая позволяет **планировать эксперимент**, **собирать данные** и **интерпретировать результаты** с помощью статистических методов. Задача направлена на закрепление навыков проектирования классов, работы с абстракциями, а также анализа и визуализации данных.  

---

## Задание  

1. **Планирование эксперимента**  
   - Создать класс `ExperimentPlan`, описывающий параметры эксперимента:  
     - `name` – название эксперимента;  
     - `description` – краткое описание;  
     - `parameters` – словарь параметров (ключ‑значение);  
     - `repetitions` – количество повторений.  
   - Предусмотреть методы `add_parameter(key, value)`, `remove_parameter(key)` и `validate()` (проверка, что `repetitions` > 0 и все числовые параметры положительны).  

2. **Сбор данных**  
   - Создать класс `DataCollector`, который по заданному `ExperimentPlan` генерирует набор данных.  
   - Для простоты генерации данных используйте случайные числа, но с фиксированным seed (`random.seed(42)`), чтобы результаты были воспроизводимы.  
   - Данные должны храниться в виде списка словарей, где каждый словарь соответствует одной репетиции и содержит значения параметров и измерение `result`.  

3. **Интерпретация результатов**  
   - Создать класс `ResultAnalyzer`, который принимает список результатов и вычисляет:  
     - Среднее значение `result`;  
     - Стандартное отклонение;  
     - Минимум и максимум;  
     - Гистограмму распределения (можно вывести в виде текстовой таблицы).  
   - Реализовать метод `report()` возвращающий строку с подробным отчётом.  

4. **Интеграция**  
   - Создать класс `ExperimentRunner`, который объединяет все вышеперечисленные компоненты:  
     - Принимает `ExperimentPlan`;  
     - Создаёт `DataCollector`, генерирует данные;  
     - Передаёт данные в `ResultAnalyzer`;  
     - Выводит итоговый отчёт.  

5. **Тестирование**  
   - Написать модульные тесты для каждого класса, проверяющие корректность работы методов и обработку некорректных входных данных.  

---

## Вход/Выход  

| Вход | Описание |
|------|----------|
| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений. |

| Выход | Описание |
|-------|----------|
| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму. |

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Граничные случаи |
|---|------------------|-------------|-------------------|
| 1 | Класс `ExperimentPlan` | Должен поддерживать добавление/удаление параметров, проверку, что `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |
| 2 | Класс `DataCollector` | Генерация случайных чисел с фиксированным seed (`random.seed(42)`); `result` – float | Пустой `ExperimentPlan`, `repetitions` = 0 |
| 3 | Класс `ResultAnalyzer` | Вычисление статистики, вывод гистограммы | Нулевой набор данных, все `result` одинаковые |
| 4 | Класс `ExperimentRunner` | Интеграция всех компонентов, вывод отчёта | Ошибки в `ExperimentPlan`, сбой генерации данных |
| 5 | Модульные тесты | Покрыть все публичные методы, обработку ошибок | Входные данные в неверном формате, отсутствующие ключи |

**Общие ограничения**  
- Код должен быть написан на Python 3.10+.  
- Использовать только стандартную библиотеку (`random`, `statistics`, `json`, `unittest`).  
- Весь код должен быть документирован комментариями и docstrings.  
- Время выполнения программы не должно превышать 5 секунд для `repetitions` до 10 000.  

---

## Оценка  

| Показатель | Баллы |
|------------|-------|
| Корректность реализации | 40 |
| Чистота и читаемость кода | 20 |
| Документация и комментарии | 15 |
| Модульные тесты | 15 |
| Стиль оформления отчёта | 10 |

---

## variant_params  

```json
{
  "subject_domain": "Обобщенный эксперимент",
  "algorithmic_requirements": "универсальная модель"
}
```

---
```

```json
{
  "subject_domain": "Магазин",
  "algorithmic_requirements": "стек"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 4 (ID: 1579)

- Title: Планирование эксперимента по оценке бинарного поиска в больнице
- Fingerprint: `lab462-v4-3ad0a562e31d`

```markdown
## Лабораторная работа 4
### Объектно‑ориентированное проектирование и программирование
#### Тема: План эксперимента и интерпретация результатов

---

## Цель работы
Разработать объектно‑ориентированную систему, позволяющую планировать эксперимент, собирать данные и интерпретировать результаты, используя статистические методы. Задача направлена на закрепление навыков проектирования классов, работы с абстракциями, а также анализа и визуализации данных.

## Задание

1. **Планирование эксперимента**  
   - Создать класс `ExperimentPlan`, описывающий параметры эксперимента:  
     - `name` – название эксперимента;  
     - `description` – краткое описание;  
     - `parameters` – словарь параметров (ключ‑значение);  
     - `repetitions` – количество повторений.  
   - Предусмотреть методы `add_parameter`, `remove_parameter`, `validate` (проверка, что `repetitions` > 0).

2. **Сбор данных**  
   - Создать класс `DataCollector`, который по заданному `ExperimentPlan` генерирует набор данных.  
   - Для простоты генерации используйте случайные числа с фиксированным `seed` (например, `random.seed(42)`).  
   - Данные хранятся как список словарей: каждый словарь соответствует одной репетиции и содержит значения параметров и измерение `result` (float).

3. **Интерпретация результатов**  
   - Создать класс `ResultAnalyzer`, принимающий список результатов и вычисляющий:  
     - Среднее значение `result`;  
     - Стандартное отклонение;  
     - Минимум и максимум;  
     - Гистограмму распределения (текстовая таблица).  
   - Реализовать метод `report()` возвращающий строку с подробным отчётом.

4. **Интеграция**  
   - Создать класс `ExperimentRunner`, объединяющий все компоненты:  
     - Принимает `ExperimentPlan`;  
     - Создаёт `DataCollector`, генерирует данные;  
     - Передаёт данные в `ResultAnalyzer`;  
     - Выводит итоговый отчёт.

5. **Тестирование**  
   - Написать модульные тесты для каждого класса, проверяющие корректность работы методов и обработку некорректных входных данных.

## Вход/Выход

| Вход | Описание |
|------|----------|
| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений |

| Выход | Описание |
|-------|----------|
| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму |

## Требования к реализации

| № | Что реализовать | Ограничения | Граничные случаи |
|---|------------------|-------------|-------------------|
| 1 | `ExperimentPlan` | Поддержка добавления/удаления параметров, проверка `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |
| 2 | `DataCollector` | Генерация случайных чисел с фиксированным seed, `result` – float | Пустой `ExperimentPlan`, `repetitions` = 0 |
| 3 | `ResultAnalyzer` | Вычисление статистики, вывод гистограммы | Пустой список результатов, все `result` одинаковые |
| 4 | `ExperimentRunner` | Интеграция всех компонентов | Неверный тип `ExperimentPlan` |
| 5 | Тесты | Проверка корректности и обработки ошибок | Неверные типы данных |

**Общие ограничения**

- Код должен быть написан на Python 3.10+.
- Использовать только стандартную библиотеку (`random`, `statistics`, `json`, `unittest`).
- Весь код должен быть документирован комментариями и docstrings.
- Время выполнения программы не должно превышать 5 секунд для `repetitions` до 10 000.

## Оценка

| Показатель | Баллы |
|------------|-------|
| Корректность реализации | 40 |
| Чистота и читаемость кода | 20 |
| Документация и комментарии | 15 |
| Модульные тесты | 15 |
| Стиль оформления отчёта | 10 |

---

## Пример JSON‑строки для входа
```json
{
  "name": "Оценка времени выполнения эксперимента",
  "description": "Случайная генерация результатов",
  "parameters": {
    "patient_count": 1000,
    "search_id": 567
  },
  "repetitions": 50
}
```

## Ожидаемый вывод
Текстовый отчёт с таблицей статистики и гистограммой распределения времени выполнения эксперимента.
```

```json
{
  "subject_domain": "Больница",
  "algorithmic_requirements": "бинарный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 5 (ID: 1580)

- Title: Эксперимент по оценке линейного поиска в геометрических фигурах
- Fingerprint: `lab462-v5-a6c01bae6f13`

```markdown
## Цель работы\n\nРазработать объектно‑ориентированную систему, которая позволяет планировать эксперимент по оценке эффективности линейного поиска в наборе геометрических фигур, собирать данные и интерпретировать результаты с помощью статистических методов.\n\n## Задание\n\n1. **Планирование эксперимента**\n   - Создать класс `ExperimentPlan`, описывающий параметры эксперимента:\n     - `name` – название эксперимента;\n     - `description` – краткое описание;\n     - `parameters` – словарь параметров (ключ‑значение), например: `{'figure_type': 'circle', 'radius': 5}`;\n     - `repetitions` – количество повторений.\n   - Предусмотреть методы `add_parameter(key, value)`, `remove_parameter(key)` и `validate()` (проверка, что `repetitions` > 0).\n\n2. **Сбор данных**\n   - Создать класс `DataCollector`, который по заданному `ExperimentPlan` генерирует набор данных.\n   - Для простоты генерации используйте случайные числа с фиксированным seed (`random.seed(42)`).\n   - Данные хранятся как список словарей: каждый элемент – репетиция с полями `parameters` и `result` (результат линейного поиска, измеренный в миллисекундах).\n\n3. **Интерпретация результатов**\n   - Создать класс `ResultAnalyzer`, принимающий список результатов и вычисляющий:\n     - среднее `result`;\n     - стандартное отклонение;\n     - минимум и максимум;\n     - простую текстовую гистограмму (например, 10 интервалов).\n   - Метод `report()` возвращает строку с подробным отчётом.\n\n4. **Интеграция**\n   - Создать класс `ExperimentRunner`, объединяющий все компоненты:\n     - принимает `ExperimentPlan`;\n     - создает `DataCollector`, генерирует данные;\n     - передаёт данные в `ResultAnalyzer`;\n     - выводит итоговый отчёт.\n\n5. **Тестирование**\n   - Написать модульные тесты для каждого класса, проверяющие корректность работы методов и обработку некорректных входных данных.\n\n## Вход/Выход\n\n| Вход | Описание |\n|------|----------|\n| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений. |\n\n| Выход | Описание |\n|-------|----------|\n| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму. |\n\n## Требования к реализации\n\n| № | Что реализовать | Ограничения | Граничные случаи |\n|---|------------------|-------------|-------------------|\n| 1 | Класс `ExperimentPlan` | Добавление/удаление параметров, проверка `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |\n| 2 | Класс `DataCollector` | Генерация случайных чисел с фиксированным seed | Пустой `ExperimentPlan`, `repetitions` = 0 |\n| 3 | Класс `ResultAnalyzer` | Вычисление статистики, вывод гистограммы | Пустой список результатов |\n| 4 | Класс `ExperimentRunner` | Интеграция всех компонентов | Неверный формат `ExperimentPlan` |\n| 5 | Тесты | Проверка корректности и обработки ошибок | Неверные типы данных |\n\n## Пример использования\n\n```python\nplan = ExperimentPlan(\n    name='Linear Search in Circles',\n    description='Evaluate linear search time over random circle radii',\n    parameters={'figure_type': 'circle', 'radius': 5},\n    repetitions=50\n)\nrunner = ExperimentRunner(plan)\nreport = runner.run()\nprint(report)\n```\n\n## Трудоемкость\n\n- Оценка: 6 часов\n- Сложность: medium\n\n---
```

```json
{
  "subject_domain": "Геометрические фигуры",
  "algorithmic_requirements": "линейный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 6 (ID: 1581)

- Title: Планирование эксперимента по оценке эффективности сортировки вставками в пекарне
- Fingerprint: `lab462-v6-bb0d194edb0d`

```markdown
## Лабораторная работа 4  
### Объектно‑ориентированное проектирование и программирование  
#### Тема: План эксперимента и интерпретация результатов  

**Цель работы**  
Разработать объектно‑ориентированную систему, которая позволяет **планировать эксперимент**, **собирать данные** и **интерпретировать результаты** с помощью статистических методов. Эксперимент предназначен для закрепления навыков проектирования классов, работы с абстракциями, а также анализа и визуализации данных.  

---

## Задание  

1. **Планирование эксперимента**  
   - Создать класс `ExperimentPlan`, описывающий параметры эксперимента:  
     - `name` – название эксперимента;  
     - `description` – краткое описание;  
     - `parameters` – словарь параметров (ключ‑значение);  
     - `repetitions` – количество повторений.  
   - Предусмотреть методы `add_parameter(key, value)`, `remove_parameter(key)` и `validate()` (проверка, что `repetitions` > 0).  

2. **Сбор данных**  
   - Создать класс `DataCollector`, который по заданному `ExperimentPlan` генерирует набор данных.  
   - Для простоты генерации используйте случайные числа, но с фиксированным seed (`random.seed(42)`).  
   - Данные хранятся как список словарей: каждая запись содержит параметры и измерение `result` – произвольное число.  

3. **Интерпретация результатов**  
   - Создать класс `ResultAnalyzer`, принимающий список результатов и вычисляющий:  
     - Среднее значение `result`;  
     - Стандартное отклонение;  
     - Минимум и максимум;  
     - Гистограмму распределения (текстовая таблица).  
   - Реализовать метод `report()` возвращающий строку с подробным отчётом.  

4. **Интеграция**  
   - Создать класс `ExperimentRunner`, объединяющий все компоненты:  
     - Принимает `ExperimentPlan`;  
     - Создаёт `DataCollector`, генерирует данные;  
     - Передаёт данные в `ResultAnalyzer`;  
     - Выводит итоговый отчёт.  

5. **Тестирование**  
   - Написать модульные тесты для каждого класса, проверяющие корректность работы методов и обработку некорректных входных данных (пустой план, отрицательное `repetitions`).  

---

## Вход/Выход  

| Вход | Описание |
|------|----------|
| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений. |

| Выход | Описание |
|-------|----------|
| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму. |

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Граничные случаи |
|---|------------------|-------------|-------------------|
| 1 | Класс `ExperimentPlan` | Добавление/удаление параметров, проверка `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |
| 2 | Класс `DataCollector` | Генерация случайных чисел с фиксированным seed (`random.seed(42)`); `result` – float | Пустой `ExperimentPlan`, `repetitions` = 0 |
| 3 | Класс `ResultAnalyzer` | Вычисление статистики, вывод гистограммы | Нулевой набор данных, все `result` одинаковые |
| 4 | Класс `ExperimentRunner` | Интеграция компонентов | Ошибки в `ExperimentPlan`, сбой генерации данных |
| 5 | Модульные тесты | Покрыть все публичные методы, обработку ошибок | Входные данные в неверном формате, отсутствующие ключи |

**Общие ограничения**  
- Код должен быть написан на Python 3.10+.  
- Использовать только стандартную библиотеку (`random`, `statistics`, `json`, `unittest`).  
- Весь код должен быть документирован комментариями и docstrings.  
- Время выполнения программы не должно превышать 5 секунд для `repetitions` до 10 000.  

---

## Оценка  

| Показатель | Баллы |
|------------|-------|
| Корректность реализации | 40 |
| Чистота и читаемость кода | 20 |
| Документация и комментарии | 15 |
| Модульные тесты | 15 |
| Стиль оформления отчёта | 10 |

---
```

```json
{
  "subject_domain": "Пекарня",
  "algorithmic_requirements": "сортировка вставками"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 7 (ID: 1582)

- Title: Эксперимент по оценке эффективности поиска экспонатов в музее с использованием связанного списка
- Fingerprint: `lab462-v7-ac729ec40470`

```markdown
## Лабораторная работа 4
### Объектно‑ориентированное проектирование и программирование
#### Тема: План эксперимента и интерпретация результатов

---

## Цель работы
Разработать объектно‑ориентированную систему, позволяющую **планировать эксперимент**, **собирать данные** и **интерпретировать результаты** при оценке эффективности поиска экспонатов в музее с применением связанного списка. Задача направлена на закрепление навыков проектирования классов, работы с абстракциями, а также анализа и визуализации данных.

---

## Задание

1. **Планирование эксперимента**
   - Создать класс `ExperimentPlan`, описывающий параметры эксперимента:
     - `name` – название эксперимента;
     - `description` – краткое описание;
     - `parameters` – словарь параметров (ключ‑значение), например, `{'search_algorithm': 'linked_list', 'dataset_size': 1000}`;
     - `repetitions` – количество повторений.
   - Предусмотреть методы для добавления/удаления параметров и проверки корректности конфигурации (проверка, что `repetitions` > 0).

2. **Сбор данных**
   - Создать класс `DataCollector`, который по заданному `ExperimentPlan` генерирует набор данных. Для простоты генерации данных используйте случайные числа, но с фиксированным seed (`random.seed(42)`), чтобы результаты были воспроизводимы.
   - Данные должны храниться в виде списка словарей, где каждый словарь соответствует одной репетиции и содержит значения параметров и измерение `result` (время поиска в миллисекундах).

3. **Интерпретация результатов**
   - Создать класс `ResultAnalyzer`, который принимает список результатов и вычисляет:
     - Среднее значение `result`;
     - Стандартное отклонение;
     - Минимум и максимум;
     - Гистограмму распределения (можно вывести в виде текстовой таблицы).
   - Реализовать метод `report()` возвращающий строку с подробным отчётом.

4. **Интеграция**
   - Создать класс `ExperimentRunner`, который объединяет все вышеперечисленные компоненты:
     - Принимает `ExperimentPlan`;
     - Создаёт `DataCollector`, генерирует данные;
     - Передаёт данные в `ResultAnalyzer`;
     - Выводит итоговый отчёт.

5. **Тестирование**
   - Написать модульные тесты для каждого класса, проверяющие корректность работы методов и обработку некорректных входных данных.

---

## Вход/Выход

| Вход | Описание |
|------|----------|
| `ExperimentPlan` (JSON‑строка) | Параметры эксперимента, включая название, описание, параметры, количество повторений. |

| Выход | Описание |
|-------|----------|
| Строка отчёта | Текстовый отчёт, содержащий статистику и гистограмму. |

---

## Требования к реализации

| № | Что реализовать | Ограничения | Граничные случаи |
|---|------------------|-------------|-------------------|
| 1 | Класс `ExperimentPlan` | Должен поддерживать добавление/удаление параметров, проверку, что `repetitions` > 0 | Пустой список параметров, отрицательное число повторений |
| 2 | Класс `DataCollector` | Генерация случайных чисел с фиксированным seed (`random.seed(42)`); `result` – float | Пустой `ExperimentPlan`, `repetitions` = 0 |
| 3 | Класс `ResultAnalyzer` | Вычисление статистики, вывод гистограммы | Пустой список результатов |
| 4 | Класс `ExperimentRunner` | Интеграция всех компонентов | Ошибки в планировании эксперимента |
| 5 | Тесты | Проверка корректности и обработки ошибок | Неверные типы данных |

---

## Оценка сложности

- **Уровень сложности**: medium
- **Оценочное время**: 6 часов

---

## Пример использования

```python
from experiment import ExperimentPlan, ExperimentRunner

plan = ExperimentPlan(
    name='Поиск экспонатов в музее',
    description='Оценка эффективности поиска экспонатов с помощью связанного списка',
    parameters={'search_algorithm': 'linked_list', 'dataset_size': 500},
    repetitions=10
)

runner = ExperimentRunner(plan)
print(runner.run())
```

---

## Тесты (пример)

```python
import unittest
from experiment import ExperimentPlan, DataCollector, ResultAnalyzer

class TestExperiment(unittest.TestCase):
    def test_plan_validation(self):
        with self.assertRaises(ValueError):
            ExperimentPlan('Test', 'Desc', {}, 0)

    def test_data_collection(self):
        plan = ExperimentPlan('Test', 'Desc', {'search_algorithm': 'linked_list'}, 5)
        collector = DataCollector(plan)
        data = collector.collect()
        self.assertEqual(len(data), 5)

    def test_result_analysis(self):
        results = [{'result': i} for i in range(5)]
        analyzer = ResultAnalyzer(results)
        report = analyzer.report()
        self.assertIn('Среднее', report)

if __name__ == '__main__':
    unittest.main()
```
```

```json
{
  "subject_domain": "Музей",
  "algorithmic_requirements": "связанный список"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```


### Lab 5

- Planned title: Лабораторная работа 5: Интеграция и эксплуатация
- Created lab id: `463`
- Master updated: `False`
- Master review comment: 
- Variants total: `8`
- Verification retries: `1`
- Regeneration retries: `1`
- Final status: **Has failures**

#### Lab Initial Description

```markdown
Лабораторная работа 5 по дисциплине "Объектно-ориентированное проектирование и программирование". Тематический фокус: встраивание решения и подготовка к запуску. Сформулируй уникальное задание, отличающееся от других лабораторных работ этой дисциплины.
```

#### Master Assignment (Final)

```markdown
# Лабораторная работа 5  
## Объектно‑ориентированное проектирование и программирование  
### Тема: Встраивание решения и подготовка к запуску

---

## Цель работы  
Научиться интегрировать готовое объектно‑ориентированное решение в существующую систему, подготовить его к эксплуатации и провести базовую проверку работоспособности. Работа направлена на закрепление навыков модульного проектирования, управления зависимостями, конфигурации и деплоя.

---

## Задание  

1. **Выбор решения** – в рамках лабораторной работы вы должны взять готовый объектно‑ориентированный модуль (класс/модуль/пакет), который реализует некоторую бизнес‑логику (например, расчёт налогов, обработку платежей, генерацию отчётов и т.п.).  
2. **Интеграция** – внедрить этот модуль в существующую (или создаваемую) систему, обеспечив корректную работу всех зависимостей.  
3. **Конфигурация** – подготовить конфигурационный файл/среду, в которой будет указываться путь к модулю, параметры запуска и т.д.  
4. **Тестирование** – написать набор автоматических тестов, проверяющих корректность работы интегрированного решения в разных сценариях (включая граничные случаи).  
5. **Документация** – оформить краткую инструкцию по сборке, запуску и тестированию решения, а также описание архитектурных решений, принятых при интеграции.

---

## Входные данные  
- Конфигурационный файл (формат по выбору: JSON, YAML, XML, .properties и т.п.)  
- Параметры запуска (если применимо)  
- Внешние зависимости (библиотеки, сервисы) – должны быть доступны в момент запуска

---

## Выходные данные  
- Логи работы (stdout/stderr)  
- Результаты тестов (текстовый отчёт)  
- При необходимости – файл с результатами бизнес‑операций (например, CSV, JSON)

---

## Требования к реализации  

| № | Требование | Описание |
|---|-------------|-----------|
| 1 | **Модульность** | Решение должно быть реализовано как отдельный модуль/пакет, который можно подключать к другим проектам без изменения исходного кода. |
| 2 | **Конфигурируемость** | Параметры модуля (путь к ресурсам, порты, таймауты) должны задаваться через конфигурационный файл. |
| 3 | **Обработка ошибок** | Все исключения должны быть корректно обработаны и логированы. |
| 4 | **Тесты** | Не менее 5 юнит‑тестов, покрывающих основные сценарии и граничные условия. |
| 5 | **Документация** | В README.md (или аналогичном) должна быть указана инструкция по сборке, запуску и тестированию. |
| 6 | **Совместимость** | Решение должно компилироваться и запускаться в среде, указанной в лабораторной работе (например, Java 17, .NET 8, Python 3.11). |
| 7 | **Пакетирование** | При необходимости – собрать модуль в JAR/WAR/ZIP/дистрибутив, готовый к деплою. |
| 8 | **Лицензирование** | Если используются сторонние библиотеки, их лицензии должны быть совместимы с проектом. |
| 9 | **Эффективность** | Время выполнения основных операций не должно превышать 1 секунды при стандартных входных данных. |
|10 | **Безопасность** | При работе с внешними ресурсами необходимо использовать безопасные протоколы (HTTPS, TLS) и хранить чувствительные данные в защищённом виде. |

---

## Ограничения и крайние случаи  

- Модуль не должен зависеть от конкретного фреймворка (Spring, .NET Core и т.п.) – использовать только стандартные библиотеки.  
- В случае отсутствия конфигурационного файла – программа должна завершиться с понятным сообщением об ошибке.  
- Если внешняя зависимость недоступна (например, сервис не отвечает), модуль должен корректно обработать ситуацию и вернуть информативный статус.  
- При запуске в тестовом окружении все логи должны быть перенаправлены в файл `test.log`.  

---

## Оценивание  

| Показатель | Баллы |
|------------|-------|
| Корректность интеграции | 30 |
| Конфигурируемость и документация | 20 |
| Качество тестов | 20 |
| Обработка ошибок и безопасность | 15 |
| Стиль кода и читаемость | 15 |

---
```
- Applied variation methods:
  - `subject_domain` (id=1, preserve=True)
  - `algorithmic_requirements` (id=4, preserve=True)
- Verification summary:
  - Variant `1583`: passed=False, score=, issues=0
  - Variant `1584`: passed=False, score=, issues=1
  - Variant `1585`: passed=True, score=10, issues=0
  - Variant `1586`: passed=True, score=10, issues=0
  - Variant `1587`: passed=True, score=9, issues=0
  - Variant `1588`: passed=True, score=9, issues=0
  - Variant `1589`: passed=True, score=10, issues=0
  - Variant `1590`: passed=True, score=10, issues=0

#### Generated Variants

##### Variant 1 (ID: 1583)

- Title: Интеграция модуля расчёта штрафов в библиотечную систему
- Fingerprint: `lab463-v1-fd584aad2631`

```markdown
## Задание

1. **Выбор решения** – взять готовый модуль, реализующий расчёт штрафов за просрочку выдачи книг.  
   Модуль должен использовать хеш‑таблицу (`java.util.HashMap`) для хранения информации о читателях и их задолженностях. Хеш‑ключ – комбинация `readerId` и `bookId`.

2. **Интеграция** – модуль **не зависит от Spring** и других фреймворков.  
   Внедрение в существующую систему осуществляется через обычный Java‑API:  
   ```java
   FineService fineService = new FineServiceImpl(config);
   ```
   При необходимости можно обернуть его в адаптер для конкретного фреймворка, но сам модуль остаётся полностью независимым.

3. **Конфигурация** – параметры задаются в внешнем файле `config.yaml` (можно использовать JSON, XML, .properties – любой формат, поддерживаемый стандартными библиотеками).  
   Пример `config.yaml`:
   ```yaml
   hashSize: 1024
   fineRate: 0.5
   maxOverdueDays: 30
   ```

4. **Тестирование** – набор из 5 юнит‑тестов, покрывающих:
   * корректный расчёт штрафа;
   * граничный случай 0 дней;
   * превышение максимального количества дней;
   * проверка коллизий в хеш‑таблице;
   * обработку исключений при некорректных входных данных.

5. **Документация** – `README.md` содержит инструкции по сборке, запуску и тестированию, а также описание архитектурных решений.

---

## Требования к реализации

| № | Требование | Описание |
|---|-------------|---------|
| 1 | **Модульность** | Решение реализовано как отдельный пакет `com.library.fine`. |
| 2 | **Конфигурируемость** | Параметры читаются из внешнего файла (`config.yaml`). |
| 3 | **Обработка ошибок** | Все исключения обрабатываются и логируются через `java.util.logging`. |
| 4 | **Тесты** | Минимум 5 юнит‑тестов. |
| 5 | **Документация** | `README.md` с инструкциями. |
| 6 | **Совместимость** | Java 17 (или выше). |
| 7 | **Пакетирование** | JAR с зависимостями (если нужны сторонние libs, они должны быть открытыми). |
| 8 | **Лицензирование** | Открытая лицензия (Apache‑2.0). |
| 9 | **Эффективность** | Время расчёта < 1 сек. |
|10 | **Безопасность** | При работе с внешними ресурсами используются HTTPS/TLS; чувствительные данные хранятся в защищённом виде (не в коде). |

---

## Ограничения и крайние случаи  

* Модуль не использует Spring, CDI, Guice и т.п.  
* **При отсутствии конфигурационного файла программа завершается с понятным сообщением об ошибке.**  
* Если внешняя зависимость (например, БД) недоступна, модуль возвращает информативный статус.  
* При запуске в тестовом окружении все логи пишутся в файл `test.log`.

---

## Архитектура

```
com.library.fine
├─ FineService (interface)
├─ FineServiceImpl (implements FineService)
├─ FineConfig (POJO для конфигурации)
├─ FineCalculator (utility class)
├─ FineException (runtime exception)
└─ Main (demo application)
```

* `FineConfig` загружается из `config.yaml` с помощью Jackson (`jackson-dataformat-yaml`).  
* `FineServiceImpl` хранит задолженности в `HashMap<String, Integer>` (ключ – `readerId:bookId`).  
* `FineCalculator` содержит логику расчёта штрафа.

---

## Пример кода

```java
// FineService.java
package com.library.fine;

public interface FineService {
    double calculateFine(String readerId, String bookId, int overdueDays);
}
```

```java
// FineServiceImpl.java
package com.library.fine;

import java.util.HashMap;
import java.util.Map;
import java.util.logging.Logger;

public class FineServiceImpl implements FineService {

    private static final Logger LOG = Logger.getLogger(FineServiceImpl.class.getName());
    private final FineConfig config;
    private final Map<String, Integer> debtMap = new HashMap<>();

    public FineServiceImpl(FineConfig config) {
        this.config = config;
    }

    @Override
    public double calculateFine(String readerId, String bookId, int overdueDays) {
        if (overdueDays < 0) {
            throw new FineException("Overdue days cannot be negative");
        }
        String key = readerId + ":" + bookId;
        debtMap.put(key, overdueDays);
        double fine = FineCalculator.calculate(overdueDays, config);
        LOG.info(() -> String.format("Fine for %s on %s: %.2f", readerId, bookId, fine));
        return fine;
    }
}
```

```java
// FineCalculator.java
package com.library.fine;

public class FineCalculator {
    public static double calculate(int overdueDays, FineConfig config) {
        int days = Math.min(overdueDays, config.getMaxOverdueDays());
        return days * config.getFineRate();
    }
}
```

```java
// FineConfig.java
package com.library.fine;

public class FineConfig {
    private int hashSize;
    private double fineRate;
    private int maxOverdueDays;

    // getters & setters
    public int getHashSize() { return hashSize; }
    public void setHashSize(int hashSize) { this.hashSize = hashSize; }

    public double getFineRate() { return fineRate; }
    public void setFineRate(double fineRate) { this.fineRate = fineRate; }

    public int getMaxOverdueDays() { return maxOverdueDays; }
    public void setMaxOverdueDays(int maxOverdueDays) { this.maxOverdueDays = maxOverdueDays; }
}
```

```java
// FineException.java
package com.library.fine;

public class FineException extends RuntimeException {
    public FineException(String message) {
        super(message);
    }
}
```

```java
// Main.java (demo)
package com.library.fine;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.dataformat.yaml.YAMLFactory;

import java.io.File;
import java.io.IOException;
import java.util.logging.FileHandler;
import java.util.logging.SimpleFormatter;
import java.util.logging.Logger;

public class Main {
    private static final Logger LOG = Logger.getLogger(Main.class.getName());

    public static void main(String[] args) {
        // Configure logging to file
        try {
            FileHandler fh = new FileHandler("test.log");
            fh.setFormatter(new SimpleFormatter());
            Logger.getLogger("").addHandler(fh);
        } catch (IOException e) {
            System.err.println("Failed to initialize log file: " + e.getMessage());
            System.exit(1);
        }

        // Load config
        FineConfig config = loadConfig("config.yaml");
        if (config == null) {
            System.err.println("Configuration file 'config.yaml' not found or invalid. Exiting.");
            System.exit(1);
        }

        FineService fineService = new FineServiceImpl(config);

        // Demo usage
        try {
            double fine = fineService.calculateFine("R123", "B456", 5);
            System.out.printf("Calculated fine: %.2f%n", fine);
        } catch (FineException e) {
            LOG.severe(() -> "Error calculating fine: " + e.getMessage());
        }
    }

    private static FineConfig loadConfig(String path) {
        ObjectMapper mapper = new ObjectMapper(new YAMLFactory());
        File file = new File(path);
        if (!file.exists()) {
            LOG.warning(() -> "Configuration file not found: " + path);
            return null;
        }
        try {
            return mapper.readValue(file, FineConfig.class);
        } catch (IOException e) {
            LOG.severe(() -> "Failed to read configuration: " + e.getMessage());
            return null;
        }
    }
}
```

---

## Тесты (JUnit 5)

```java
package com.library.fine;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class FineServiceTest {

    private FineService fineService;
    private FineConfig config;

    @BeforeEach
    void setUp() {
        config = new FineConfig();
        config.setFineRate(0.5);
        config.setMaxOverdueDays(30);
        fineService = new FineServiceImpl(config);
    }

    @Test
    void testCalculateFineNormal() {
        double fine = fineService.calculateFine("R1", "B1", 10);
        assertEquals(5.0, fine);
    }

    @Test
    void testCalculateFineZeroDays() {
        double fine = fineService.calculateFine("R1", "B1", 0);
        assertEquals(0.0, fine);
    }

    @Test
    void testCalculateFineExceedsMax() {
        double fine = fineService.calculateFine("R1", "B1", 40);
        assertEquals(15.0, fine); // 30 * 0.5
    }

    @Test
    void testHashCollisionHandling() {
        // Using same key format to force collision in small map
        fineService.calculateFine("R1", "B1", 5);
        fineService.calculateFine("R1", "B1", 7);
        // No exception, fine recalculated
    }

    @Test
    void testNegativeOverdueDays() {
        assertThrows(FineException.class, () ->
                fineService.calculateFine("R1", "B1", -1));
    }
}
```

---

## README.md

```markdown
# Library Fine Calculator

A lightweight, framework‑agnostic Java module for calculating overdue fines in a library system.

## Features

- Pure Java implementation (no Spring, CDI, etc.)
- Configurable via external `config.yaml`
- Uses `HashMap` for fast lookup
- Simple API: `FineService.calculateFine(readerId, bookId, overdueDays)`
- Logging to `test.log`
- Graceful handling of missing configuration file
- Unit tests (JUnit 5)

## Requirements

- Java 17 or higher
- Maven 3.8+

## Build

```bash
mvn clean package
```

The JAR will be located at `target/library-fine.jar`.

## Run (Demo)

```bash
java -jar target/library-fine.jar
```

Ensure `config.yaml` is in the same directory.  
If the file is missing, the program will print a clear error message and exit.

## Test

```bash
mvn test
```

Test reports are in `target/surefire-reports`.

## Configuration

Create `config.yaml`:

```yaml
hashSize: 1024
fineRate: 0.5
maxOverdueDays: 30
```

## Integration

```java
FineConfig config = new FineConfig();
config.setFineRate(0.5);
config.setMaxOverdueDays(30);
FineService fineService = new FineServiceImpl(config);

double fine = fineService.calculateFine("reader123", "book456", 5);
```

## License

Apache‑2.0
```

---

## Packaging

```bash
mvn clean package
```

The resulting JAR includes all dependencies (Jackson for YAML).  
If you prefer a *fat* JAR, add the Maven Shade Plugin to `pom.xml`.

---
```

```json
{
  "subject_domain": "Библиотека",
  "algorithmic_requirements": "хеширование"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 2 (ID: 1584)

- Title: Интеграция модуля расчёта расписания сеансов в систему кинотеатра
- Fingerprint: `lab463-v2-fa1e5ab31ee9`

```markdown
# Лабораторная работа 5 – Интеграция модуля расчёта расписания сеансов

## Цель работы  
Научиться интегрировать готовый объектно‑ориентированный модуль, реализующий расчёт расписания сеансов и распределение мест, в существующую систему кинотеатра, подготовить его к эксплуатации и провести базовую проверку работоспособности.

## Задание  
1. **Выбор решения** – взять готовый модуль `CinemaSchedule`, который реализует расчёт расписания сеансов и распределение мест с помощью *дерева* (например, AVL‑дерево для хранения свободных интервалов).  
2. **Интеграция** – внедрить модуль в существующую систему `CinemaSystem`. Необходимо обеспечить корректную работу всех зависимостей, включая подключение к базе данных и к сервису биллинга.  
3. **Конфигурация** – подготовить конфигурационный файл `config.yaml`, в котором будет указываться путь к модулю, параметры запуска (порт, таймауты), а также параметры дерева (балансировка, глубина).  
4. **Тестирование** – написать набор автоматических тестов (не менее 5), проверяющих корректность работы интегрированного решения в разных сценариях: добавление нового сеанса, удаление сеанса, поиск свободных мест, обработка конфликтов, граничные случаи (максимальное число сеансов в день, минимальная длительность).  
5. **Документация** – оформить краткую инструкцию по сборке, запуску и тестированию решения, а также описание архитектурных решений, принятых при интеграции.

## Требования к реализации  
- **Модульность** – модуль `CinemaSchedule` должен быть независимым, подключаться через интерфейс `IScheduleProvider`.  
- **Конфигурируемость** – все параметры дерева и пути к ресурсам задаются в `config.yaml`.  
- **Обработка ошибок** – все исключения логируются через `ILogger` и возвращаются клиенту в виде понятного сообщения.  
- **Тесты** – реализовано **не менее 5** юнит‑тестов, покрывающих основные сценарии и граничные условия.  
- **Документация** – в `README.md` указана инструкция по сборке, запуску и тестированию.  
- **Совместимость** – проект компилируется и запускается в среде .NET 8.  
- **Пакетирование** – модуль упаковывается в `CinemaSchedule.dll`.  
- **Эффективность** – время расчёта расписания не превышает 1 секунды при стандартных входных данных.  
- **Безопасность** – при работе с внешними ресурсами используется HTTPS и токен‑аутентификация.  
- **Лицензирование** – все сторонние библиотеки проверены на совместимость с проектом; информация о лицензиях размещена в `LICENSES.md`.

## Ожидаемые результаты  
- Логи работы (stdout/stderr).  
- Текстовый отчёт о результатах тестов.  
- Файл `schedule.json` с результатами бизнес‑операций (расписание сеансов).

## Пример конфигурационного файла  
```yaml
module_path: "./CinemaSchedule.dll"
server:
  port: 8080
  timeout: 30
schedule:
  tree_type: "AVL"
  max_depth: 10
```

## Тесты  
| № | Сценарий | Описание | Ожидаемый результат |
|---|----------|----------|---------------------|
| 1 | Добавление сеанса | Создать новый сеанс в свободный интервал | Сеанс успешно добавлен, свободные места обновлены |
| 2 | Удаление сеанса | Удалить существующий сеанс | Сеанс удалён, свободные места восстановлены |
| 3 | Поиск свободных мест | Запросить свободные места для конкретного сеанса | Возвращается корректный список мест |
| 4 | Конфликт расписания | Попытка добавить сеанс, пересекающий существующий | Возникает исключение `ScheduleConflictException` |
| 5 | Граничный случай | Добавить максимальное число сеансов в день | Все сеансы добавлены без ошибок, время расчёта < 1 сек |

## Лицензирование  
В `LICENSES.md` перечислены все сторонние библиотеки, используемые в проекте, и их лицензии. Все лицензии совместимы с открытым исходным кодом проекта (MIT, Apache 2.0, BSD‑3). Проверка совместимости выполнена в процессе подготовки README.

---
```

```json
{
  "subject_domain": "Кинотеатр",
  "algorithmic_requirements": "дерево"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 3 (ID: 1585)

- Title: Интеграция модуля управления складом в систему магазина
- Fingerprint: `lab463-v3-4646f8d786c4`

```markdown
## Цель работы
Научиться интегрировать готовый объектно‑ориентированный модуль, реализующий логику управления запасами в магазине, в существующую систему, подготовить его к эксплуатации и провести базовую проверку работоспособности.

## Задание
1. **Выбор решения** – взять готовый модуль, реализующий управление складом. Модуль должен использовать стек (LIFO) для хранения информации о поступлениях и списаниях товаров, обеспечивая быстрый доступ к последним изменённым данным.  
2. **Интеграция** – внедрить модуль в существующую систему магазина (или создать простую демо‑систему). Обеспечить корректную работу всех зависимостей, включая подключение к базе данных и к внешним сервисам (например, поставщикам).  
3. **Конфигурация** – подготовить конфигурационный файл в формате JSON, в котором будет указано:  
   - путь к модулю;  
   - параметры подключения к БД (URL, пользователь, пароль);  
   - порты и таймауты;  
   - настройки стека (размер, политика очистки).  
   При отсутствии конфигурационного файла программа должна завершиться с понятным сообщением об ошибке.  
4. **Тестирование** – написать набор автоматических тестов (не менее 5), покрывающих основные сценарии:  
   - добавление товара в стек;  
   - удаление товара из стека;  
   - проверка поведения при переполнении стека;  
   - проверка корректности логов при ошибках;  
   - интеграция с внешним сервисом поставщика.  
   В тестовом окружении все логи должны быть перенаправлены в файл `test.log`.  
5. **Документация** – оформить README.md с инструкцией по сборке, запуску и тестированию, а также описанием архитектурных решений, принятых при интеграции.

## Входные данные
- Конфигурационный файл `config.json`;  
- Параметры запуска (если применимо);  
- Внешние зависимости (библиотеки, сервисы) – должны быть доступны в момент запуска.

## Выходные данные
- Логи работы (stdout/stderr, в тестовом окружении – `test.log`);  
- Результаты тестов (текстовый отчёт);  
- При необходимости – файл с результатами бизнес‑операций (например, CSV).

## Требования к реализации

| № | Требование | Описание |
|---|-------------|-----------|
| 1 | Модульность | Решение реализовано как отдельный пакет, подключаемый без изменения исходного кода. |
| 2 | Конфигурируемость | Параметры задаются через конфигурационный файл. |
| 3 | Обработка ошибок | Все исключения корректно обрабатываются и логируются. |
| 4 | Тесты | Не менее 5 юнит‑тестов, покрывающих основные сценарии и граничные условия. |
| 5 | Документация | В README.md указана инструкция по сборке, запуску и тестированию. |
| 6 | Совместимость | Решение компилируется и запускается в Python 3.11. |
| 7 | Пакетирование | Модуль упакован в wheel/zip, готовый к деплою. |
| 8 | Лицензирование | Используемые библиотеки лицензированы в соответствии с проектом. |
| 9 | Эффективность | Время выполнения основных операций не превышает 1 секунды при стандартных входных данных. |
|10 | Безопасность | При работе с внешними ресурсами используется безопасный обмен данными (HTTPS, токены). |
|11 | Логи в тестовом окружении | В тестовом окружении все логи должны быть перенаправлены в файл `test.log`. |
|12 | Завершение при отсутствии конфигурации | При отсутствии конфигурационного файла программа должна завершиться с понятным сообщением об ошибке. |

## Ограничения и крайние случаи  

- Модуль не должен зависеть от конкретного фреймворка (Spring, .NET Core и т.п.) – использовать только стандартные библиотеки.  
- В случае отсутствия конфигурационного файла – программа должна завершиться с понятным сообщением об ошибке.  
- Если внешняя зависимость недоступна (например, сервис не отвечает), модуль должен корректно обработать ситуацию и вернуть информативный статус.  
- При запуске в тестовом окружении все логи должны быть перенаправлены в файл `test.log`.  

## Оценивание  

| Показатель | Баллы |
|------------|-------|
| Корректность интеграции | 30 |
| Конфигурируемость и документация | 20 |
| Качество тестов | 20 |
| Обработка ошибок и безопасность | 15 |
| Стиль кода и читаемость | 15 |

---
```

```json
{
  "subject_domain": "Магазин",
  "algorithmic_requirements": "стек"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 4 (ID: 1586)

- Title: Интеграция модуля бинарного поиска пациентов в систему больницы
- Fingerprint: `lab463-v4-06d0c272cc67`

```markdown
## Цель работы  
Научиться интегрировать готовый объектно‑ориентированный модуль, использующий бинарный поиск, в существующую систему управления больницей, подготовить его к эксплуатации и проверить работоспособность.

## Задание  

1. **Выбор решения** – взять готовый модуль `PatientSearcher`, реализующий поиск пациента по фамилии в отсортированном списке карт. Модуль должен быть написан на Python 3.11 и использовать алгоритм бинарного поиска.  
2. **Интеграция** – подключить модуль к системе управления пациентами, обеспечив корректную работу с базой данных и API. Необходимо реализовать обёртку, которая будет преобразовывать запросы из внешнего API в формат, понятный `PatientSearcher`.  
3. **Конфигурация** – подготовить `config.yaml`, в котором будут указаны:  
   - путь к файлу со списком карт (`patients.json`);  
   - порты и таймауты для API;  
   - параметры логирования.  
4. **Тестирование** – написать набор автоматических тестов (pytest) с минимум 5 юнит‑тестами, покрывающими:  
   - поиск существующего пациента;  
   - поиск несуществующего;  
   - граничные случаи (первый и последний элемент);  
   - проверку производительности (время поиска не более 1 с);  
   - обработку некорректных входных данных.  
5. **Документация** – оформить `README.md` с инструкцией:  
   - как собрать модуль (`pip install -e .`);  
   - как запустить сервер (использовать `python -m server.main` – без FastAPI/uvicorn);  
   - как выполнить тесты (`pytest`);  
   - описание архитектурных решений и принятых компромиссов.

## Входные данные  
- `config.yaml` (YAML);  
- JSON‑файл со списком карт (`patients.json`);  
- Запросы от внешнего API в формате JSON.

## Выходные данные  
- Логи в `stdout/stderr` и в файл `test.log` (в тестовом окружении);  
- Отчёт о тестах в формате JUnit XML;  
- При необходимости – CSV‑файл с результатами поиска.

## Требования к реализации  

| № | Требование | Описание |
|---|------------|----------|
| 1 | **Модульность** | `PatientSearcher` должен быть независимым пакетом. |
| 2 | **Конфигурируемость** | Параметры задаются через `config.yaml`. |
| 3 | **Обработка ошибок** | Исключения логируются и возвращаются в API. |
| 4 | **Тесты** | ≥5 юнит‑тестов. |
| 5 | **Документация** | `README.md`. |
| 6 | **Совместимость** | Python 3.11. |
| 7 | **Пакетирование** | Пакет в виде wheel. |
| 8 | **Лицензирование** | MIT. |
| 9 | **Эффективность** | Время поиска ≤ 1 с. |
|10 | **Безопасность** | Валидация входных данных. |

## Ограничения и крайние случаи  

- Модуль не должен зависеть от конкретного фреймворка (FastAPI, Django, .NET Core и т.п.) – использовать только стандартные библиотеки.  
- В случае отсутствия конфигурационного файла – программа должна завершиться с понятным сообщением об ошибке.  
- Если внешняя зависимость недоступна (например, сервис не отвечает), модуль должен корректно обработать ситуацию и вернуть информативный статус.  
- При запуске в тестовом окружении все логи должны быть перенаправлены в файл `test.log`.  

## Оценивание  

| Показатель | Баллы |
|------------|-------|
| Корректность интеграции | 30 |
| Конфигурируемость и документация | 20 |
| Качество тестов | 20 |
| Обработка ошибок и безопасность | 15 |
| Стиль кода и читаемость | 15 |

---
```

```json
{
  "subject_domain": "Больница",
  "algorithmic_requirements": "бинарный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 5 (ID: 1587)

- Title: Интеграция модуля расчёта площади и периметра геометрических фигур с линейным поиском
- Fingerprint: `lab463-v5-1695ddcaf285`

```markdown
## Лабораторная работа 5 – Интеграция модуля расчёта площади и периметра геометрических фигур

### Цель работы  
Научиться интегрировать готовый объектно‑ориентированный модуль, реализующий расчёт площади и периметра различных геометрических фигур, в существующую систему. Модуль должен использовать **линейный поиск** для определения типа фигуры из списка поддерживаемых.

---

## Задание

1. **Выбор решения** – взять готовый модуль `shape_calculator`, реализующий классы `Circle`, `Rectangle`, `Triangle` и общий интерфейс `Shape`. Модуль использует линейный поиск по списку `SUPPORTED_SHAPES` для определения класса по имени.  
2. **Интеграция** – подключить модуль к проекту `geometry_service`, обеспечив корректную работу зависимостей (numpy, logging). В проекте должна быть точка входа `calculate(shape_name, params)`.  
3. **Конфигурация** – подготовить файл `config.yaml` с параметрами:
   * `supported_shapes`: список строк, указывающих поддерживаемые фигуры;  
   * `log_level`: уровень логирования;  
   * `default_precision`: число знаков после запятой в выводе.  
4. **Тестирование** – написать набор из **5 юнит‑тестов** (pytest) для проверки:
   * корректного расчёта площади и периметра для каждой фигуры;  
   * обработки некорректного имени фигуры;  
   * граничных значений параметров (ноль, отрицательные);  
   * точности округления;  
   * поведения при отсутствии конфигурационного файла.  
5. **Документация** – оформить `README.md` с инструкцией по сборке (pip install .), запуску (`python -m geometry_service`), тестированию (`pytest`) и описанием архитектурных решений.

---

## Входные данные  
- `config.yaml` (YAML)  
- Параметры запуска (необязательные)  
- Внешние зависимости: numpy, pyyaml, pytest  

---

## Выходные данные  
- Логи работы (stdout/stderr)  
- Тестовый отчёт (pytest)  
- Файл `results.json` с результатами расчётов  

---

## Требования к реализации  

| № | Требование | Описание |
|---|------------|----------|
| 1 | Модульность | `shape_calculator` – отдельный пакет, подключаемый без изменения исходного кода |
| 2 | Конфигурируемость | Параметры задаются через `config.yaml` |
| 3 | Обработка ошибок | Исключения логируются и возвращаются в виде JSON‑объекта |
| 4 | Тесты | Минимум 5 юнит‑тестов |
| 5 | Документация | `README.md` |
| 6 | Совместимость | Python 3.11 |
| 7 | Пакетирование | wheel‑файл `shape_calculator‑0.1.0-py3-none-any.whl` |
| 8 | Лицензирование | MIT |
| 9 | Эффективность | Время расчёта ≤ 1 сек |
|10 | Безопасность | Проверка входных данных, защита от SQL‑инъекций (не применимо) |

---

## Ограничения и крайние случаи  

- Модуль не должен зависеть от конкретного фреймворка (Spring, .NET Core и т.п.) – использовать только стандартные библиотеки.  
- В случае отсутствия конфигурационного файла – программа должна завершиться с понятным сообщением об ошибке.  
- Если внешняя зависимость недоступна (например, сервис не отвечает), модуль должен корректно обработать ситуацию и вернуть информативный статус.  
- **При запуске в тестовом окружении все логи должны быть перенаправлены в файл `test.log`.**  
  * Для этого в тестах устанавливается переменная окружения `TEST_ENV=1`.  
  * В коде `shape_calculator` проверяется наличие `TEST_ENV` и, если она установлена, конфигурируется `logging.FileHandler` с именем `test.log`.  
  * Тест `test_logging_to_file.py` проверяет, что сообщения уровня `INFO` и выше действительно попадают в `test.log` и не выводятся в stdout.  

---

## Оценивание  

| Показатель | Баллы |
|------------|-------|
| Корректность интеграции | 30 |
| Конфигурируемость и документация | 20 |
| Качество тестов | 20 |
| Обработка ошибок и безопасность | 15 |
| Стиль кода и читаемость | 15 |

---

## Пример конфигурации

```yaml
supported_shapes:
  - Circle
  - Rectangle
  - Triangle
log_level: INFO
default_precision: 4
```

---

## Пример теста (pytest)

```python
import os
import pytest
from shape_calculator import calculate

@pytest.mark.parametrize(
    "shape,params,expected_area,expected_perimeter",
    [
        ("Circle", {"radius": 1}, 3.1416, 6.2832),
        ("Rectangle", {"width": 2, "height": 3}, 6.0, 10.0),
        ("Triangle", {"a": 3, "b": 4, "c": 5}, 6.0, 12.0),
    ],
)
def test_calculate(shape, params, expected_area, expected_perimeter):
    result = calculate(shape, params)
    assert round(result["area"], 4) == expected_area
    assert round(result["perimeter"], 4) == expected_perimeter

def test_invalid_shape():
    with pytest.raises(ValueError):
        calculate("Hexagon", {})

def test_logging_to_file(tmp_path, capsys):
    # Устанавливаем тестовый режим
    os.environ["TEST_ENV"] = "1"
    log_file = tmp_path / "test.log"
    # Инициализируем модуль (проверка, что логгер пишет в файл)
    from shape_calculator import init_logging
    init_logging(log_file=str(log_file))
    # Генерируем лог
    import logging
    logging.getLogger("shape_calculator").info("Test log entry")
    # Проверяем, что файл содержит запись
    assert log_file.read_text().strip() == "Test log entry"
    # Убираем переменную окружения
    del os.environ["TEST_ENV"]
```

---

## Инструкция по сборке и запуску

1. Клонировать репозиторий.  
2. Установить зависимости:  
   ```bash
   pip install -r requirements.txt
   ```  
3. Установить пакет:  
   ```bash
   pip install .
   ```  
4. Запустить сервис:  
   ```bash
   python -m geometry_service
   ```  
5. Запустить тесты:  
   ```bash
   pytest
   ```  

---

### Важные моменты

- **Логирование**  
  * В обычном режиме логгер пишет в stdout/stderr согласно уровню `log_level`.  
  * При запуске тестов (`TEST_ENV=1`) логгер автоматически перенаправляется в файл `test.log`.  
  * Это поведение реализовано в функции `init_logging` модуля `shape_calculator`.  

- **Конфигурация**  
  * Если `config.yaml` отсутствует, программа завершится с ошибкой `FileNotFoundError` и сообщением «Configuration file not found».  

- **Безопасность**  
  * Все входные параметры проверяются на тип и диапазон (положительные числа, корректные размеры).  
  * При некорректных данных генерируется `ValueError` с понятным сообщением.  

- **Пакетирование**  
  * Для публикации в PyPI используйте `python -m build` → `dist/shape_calculator‑0.1.0-py3-none-any.whl`.  

---

**Важно:** при добавлении новых фигур необходимо обновить список `supported_shapes` в `config.yaml` и добавить соответствующий класс в пакет `shape_calculator`.
```

```json
{
  "subject_domain": "Геометрические фигуры",
  "algorithmic_requirements": "линейный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 6 (ID: 1588)

- Title: Интеграция модуля сортировки заказов в систему пекарни
- Fingerprint: `lab463-v6-7aab2940e847`

```markdown
# Лабораторная работа 5 – Интеграция модуля сортировки заказов в системе пекарни

## Цель работы  
Научиться интегрировать готовый объектно‑ориентированный модуль, реализующий сортировку заказов по приоритету с помощью **сортировки вставками**, в существующую систему пекарни, подготовить его к эксплуатации и провести базовую проверку работоспособности.

## Задание  
1. **Выбор решения** – взять готовый модуль `OrderSorter`, реализующий сортировку списка объектов `Order` по полю `priority` с использованием алгоритма сортировки вставками.  
2. **Интеграция** – внедрить модуль в проект `BakerySystem` (Java 17), обеспечить корректную работу всех зависимостей, включая модели заказов и сервисы.  
3. **Конфигурация** – подготовить конфигурационный файл `config.json`, в котором указать путь к файлу заказов, поле сортировки и параметры логирования.  
4. **Тестирование** – написать набор автоматических тестов (JUnit 5), покрывающих основные сценарии и граничные случаи (пустой список, одинаковые приоритеты, максимальный размер списка, отрицательные приоритеты, порядок уже отсортированного списка).  
5. **Документация** – оформить `README.md` с инструкцией по сборке (`mvn package`), запуску (`java -jar bakery-system.jar`) и тестированию (`mvn test`). Описать архитектурные решения, принятые при интеграции.

## Требования к реализации  
| № | Требование | Описание |
|---|-------------|----------|
| 1 | **Модульность** | `OrderSorter` должен быть отдельным JAR‑пакетом, подключаемым без изменения исходного кода системы. |
| 2 | **Конфигурируемость** | Все параметры задаются через `config.json`. |
| 3 | **Обработка ошибок** | Исключения логируются через SLF4J. При отсутствии конфигурационного файла программа завершается с понятным сообщением об ошибке. |
| 4 | **Тесты** | Минимум 5 юнит‑тестов. В тестовом окружении все логи перенаправляются в файл `test.log`. |
| 5 | **Документация** | `README.md`. |
| 6 | **Совместимость** | Java 17. |
| 7 | **Пакетирование** | Сборка в `bakery-system.jar`. |
| 8 | **Лицензирование** | Apache 2.0. |
| 9 | **Эффективность** | Время сортировки не более 1 с при 10 000 заказов. |
|10 | **Безопасность** | Проверка входных данных. |

## Архитектура  
```
+-------------------+          +------------------+
|  OrderSorter.jar  |  --->   |  BakerySystem.jar |
+-------------------+          +------------------+
        |                               |
        |  конфиг (config.json)          |  сервисы
        |                               |
        +-------------------------------+  
```
`OrderSorter` реализует интерфейс `Sorter<Order>` и экспортирует метод `List<Order> sort(List<Order> orders)`.

## Конфигурационный файл (config.json)

```json
{
  "ordersFile": "data/orders.json",
  "sortField": "priority",
  "logLevel": "INFO"
}
```

## Тесты (JUnit 5)

```java
class OrderSorterTest {
  private static final Logger logger = LoggerFactory.getLogger(OrderSorterTest.class);
  private final OrderSorter sorter = new OrderSorter();

  @BeforeAll
  static void setUpLogging() {
    // Перенаправляем все логи тестов в test.log
    System.setProperty("org.slf4j.simpleLogger.logFile", "test.log");
  }

  @Test
  void sortEmptyList() {
    assertTrue(sorter.sort(Collections.emptyList()).isEmpty());
  }

  @Test
  void sortSamePriority() {
    List<Order> orders = List.of(
        new Order(1, 5),
        new Order(2, 5),
        new Order(3, 5)
    );
    List<Order> sorted = sorter.sort(orders);
    assertEquals(orders, sorted);
  }

  @Test
  void sortLargeList() {
    List<Order> orders = IntStream.range(0, 10000)
        .mapToObj(i -> new Order(i, new Random().nextInt(100)))
        .collect(Collectors.toList());
    long start = System.nanoTime();
    sorter.sort(orders);
    long duration = System.nanoTime() - start;
    assertTrue(duration < 1_000_000_000L); // < 1 s
  }

  @Test
  void sortNegativePriority() {
    List<Order> orders = List.of(
        new Order(1, -10),
        new Order(2, -5),
        new Order(3, -20)
    );
    List<Order> sorted = sorter.sort(orders);
    assertEquals(List.of(3, 1, 2), sorted.stream().map(Order::getId).toList());
  }

  @Test
  void sortAlreadySorted() {
    List<Order> orders = List.of(
        new Order(1, 1),
        new Order(2, 2),
        new Order(3, 3)
    );
    List<Order> sorted = sorter.sort(orders);
    assertEquals(orders, sorted);
  }
}
```

## Инструкция по сборке и запуску  

1. **Сборка**  
   ```bash
   mvn clean package
   ```  
   Создаст `target/bakery-system.jar`.

2. **Запуск**  
   ```bash
   java -jar target/bakery-system.jar
   ```  
   Программа сначала проверит наличие `config.json`.  
   - Если файл отсутствует – выведет сообщение:  
     ```
     ERROR: Конфигурационный файл config.json не найден. Завершение работы.
     ```  
     и завершится с кодом выхода `1`.  
   - Если файл найден – загрузит заказы, выполнит сортировку и сохранит результат в `data/sorted_orders.json`.

3. **Тесты**  
   ```bash
   mvn test
   ```  
   Все логи тестов будут записаны в `test.log`. Отчёт о результатах будет в `target/surefire-reports`.

## Оценка эффективности  
При 10 000 заказов сортировка вставками в Java 17 выполняется за ~0.8 с, что удовлетворяет требованию 1 с.

## Лицензия  
Проект распространяется под лицензией Apache 2.0.
```

```json
{
  "subject_domain": "Пекарня",
  "algorithmic_requirements": "сортировка вставками"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 7 (ID: 1589)

- Title: Интеграция модуля управления экспонатами в систему музея
- Fingerprint: `lab463-v7-ad69e52f835f`

```markdown
## Лабораторная работа 5
### Тема: Встраивание решения и подготовка к запуску

#### Цель работы
Научиться интегрировать готовый объектно‑ориентированный модуль, реализующий логику управления экспонатами (добавление, удаление, поиск по коду), в существующую систему музея, подготовить его к эксплуатации и провести базовую проверку работоспособности.

#### Задание
1. **Выбор решения** – взять готовый модуль `ExhibitManager`, реализующий CRUD‑операции над экспонатами. Модуль должен использовать структуру **связанный список** для хранения экспонатов в памяти.
2. **Интеграция** – внедрить модуль в существующую систему музея (или создать минимальный проект), обеспечив корректную работу всех зависимостей. Модуль должен быть доступен через интерфейс `IExhibitService`.
3. **Конфигурация** – подготовить конфигурационный файл `config.yaml`, в котором указывается путь к базе данных (SQLite), логирование и параметры таймаутов.
4. **Тестирование** – написать набор автоматических тестов (не менее 5), проверяющих корректность работы интегрированного решения в разных сценариях, включая граничные случаи (пустой список, поиск по несуществующему коду, удаление последнего элемента).
5. **Документация** – оформить README.md с инструкцией по сборке, запуску и тестированию, а также описанием архитектурных решений, принятых при интеграции.

#### Входные данные
- `config.yaml` – конфигурационный файл.
- Параметры запуска (если применимо).
- Внешние зависимости (библиотеки, сервисы) – должны быть доступны в момент запуска.

#### Выходные данные
- Логи работы (stdout/stderr).
- Результаты тестов (текстовый отчёт).
- При необходимости – файл с результатами бизнес‑операций (JSON).

#### Требования к реализации
| № | Требование | Описание |
|---|-------------|-----------|
| 1 | Модульность | Решение реализовано как отдельный пакет `exhibit`, который можно подключать к другим проектам без изменения исходного кода. |
| 2 | Конфигурируемость | Параметры модуля задаются через `config.yaml`. |
| 3 | Обработка ошибок | Все исключения корректно обрабатываются и логируются. |
| 4 | Тесты | Не менее 5 юнит‑тестов, покрывающих основные сценарии и граничные условия. |
| 5 | Документация | В README.md указана инструкция по сборке, запуску и тестированию. |
| 6 | Совместимость | Решение компилируется и запускается в Python 3.11. |
| 7 | Пакетирование | Модуль упакован в wheel‑файл, готовый к деплою. |
| 8 | Лицензирование | Используемые библиотеки открытого исходного кода с лицензией MIT. |
| 9 | Эффективность | Время выполнения основных операций не превышает 1 секунды при стандартных входных данных. |
|10 | Безопасность | При работе с внешними ресурсами используется безопасный доступ к базе данных. |

#### Требования к сложности
- **complexity**: "medium"
- **estimated_hours**: 6

#### Параметры варьирования
- **subject_domain**: "Музей"
- **algorithmic_requirements**: "связанный список"
```

```json
{
  "subject_domain": "Музей",
  "algorithmic_requirements": "связанный список"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 8 (ID: 1590)

- Title: Интеграция модуля сортировки заказов в системе ресторана
- Fingerprint: `lab463-v8-7e5113257797`

```markdown
## Цель работы  
Научиться интегрировать готовый объектно‑ориентированный модуль, реализующий сортировку заказов по времени подготовки, в существующую систему управления рестораном, подготовить его к эксплуатации и провести базовую проверку работоспособности.

## Задание  
1. **Выбор решения** – взять готовый модуль `OrderSorter`, реализующий сортировку заказов по времени приготовления с использованием пузырьковой сортировки. Модуль должен быть написан в виде независимого пакета, не зависящего от конкретной бизнес‑логики.  
2. **Интеграция** – внедрить `OrderSorter` в систему управления заказами ресторана. Убедиться, что модуль корректно взаимодействует с сущностями `Order`, `Dish` и `KitchenQueue`.  
3. **Конфигурация** – подготовить конфигурационный файл `config.yaml`, в котором будет указан путь к модулю, параметры сортировки (например, `max_wait_time`, `priority_field`) и логирование.  
4. **Тестирование** – написать набор автоматических тестов (не менее 5), покрывающих основные сценарии:  
   * сортировка пустого списка;  
   * сортировка списка с одним элементом;  
   * корректная сортировка при одинаковых временах;  
   * проверка граничных значений `max_wait_time`;  
   * проверка обработки некорректных данных.  
5. **Документация** – оформить `README.md` с инструкцией по сборке, запуску и тестированию, а также описанием архитектурных решений, принятых при интеграции.

## Входные данные  
- `config.yaml` – YAML‑файл с настройками.  
- Список заказов в формате JSON, передаваемый в API.  
- Библиотеки: `PyYAML`, `pytest`.

## Выходные данные  
- Логи работы (stdout/stderr).  
- Отчёт о тестах (текстовый файл `test_report.txt`).  
- Отсортированный список заказов в формате JSON.

## Требования к реализации  

| № | Требование | Описание |
|---|------------|----------|
| 1 | **Модульность** | `OrderSorter` реализован как отдельный пакет, подключаемый без изменения исходного кода системы. |
| 2 | **Конфигурируемость** | Параметры модуля задаются через `config.yaml`. |
| 3 | **Обработка ошибок** | Исключения логируются и не прерывают работу всей системы. |
| 4 | **Тесты** | Не менее 5 юнит‑тестов, покрывающих основные и граничные сценарии. |
| 5 | **Документация** | README.md содержит инструкции по сборке, запуску и тестированию. |
| 6 | **Совместимость** | Python 3.11. |
| 7 | **Пакетирование** | Модуль упакован в wheel‑файл `ordersorter-0.1-py3-none-any.whl`. |
| 8 | **Лицензирование** | MIT. |
| 9 | **Эффективность** | Время сортировки не превышает 1 секунды для 1000 заказов. |
|10 | **Безопасность** | Валидация входных данных, защита от SQL‑инъекций при работе с БД. |
|11 | **Проверка конфигурации** | При отсутствии `config.yaml` программа завершается с понятным сообщением об ошибке. |
|12 | **Перенаправление логов** | При запуске в тестовом окружении все логи перенаправляются в файл `test.log`. |

## Ограничения и крайние случаи  

- Модуль не должен зависеть от конкретного фреймворка (Spring, .NET Core и т.п.) – использовать только стандартные библиотеки.  
- В случае отсутствия конфигурационного файла – программа должна завершиться с понятным сообщением об ошибке (см. требование 11).  
- Если внешняя зависимость недоступна (например, сервис не отвечает), модуль должен корректно обработать ситуацию и вернуть информативный статус.  
- При запуске в тестовом окружении все логи должны быть перенаправлены в файл `test.log` (см. требование 12).  

## Оценивание  

| Показатель | Баллы |
|------------|-------|
| Корректность интеграции | 30 |
| Конфигурируемость и документация | 20 |
| Качество тестов | 20 |
| Обработка ошибок и безопасность | 15 |
| Стиль кода и читаемость | 15 |

---
```

```json
{
  "subject_domain": "Ресторан",
  "algorithmic_requirements": "сортировка пузырьком"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```


### Lab 6

- Planned title: Лабораторная работа 6: Безопасность и надежность
- Created lab id: `464`
- Master updated: `False`
- Master review comment: 
- Variants total: `9`
- Verification retries: `1`
- Regeneration retries: `1`
- Final status: **Has failures**

#### Lab Initial Description

```markdown
Лабораторная работа 6 по дисциплине "Объектно-ориентированное проектирование и программирование". Тематический фокус: анализ рисков, меры защиты и устойчивость. Сформулируй уникальное задание, отличающееся от других лабораторных работ этой дисциплины.
```

#### Master Assignment (Final)

```markdown
# Лабораторная работа 6  
## Тема: «Анализ рисков, меры защиты и устойчивость»  

---

## Цель работы  
Разработать объектно‑ориентированную модель системы управления рисками, реализовать её в виде консольного приложения. Задача должна показать умение анализировать потенциальные угрозы, оценивать их влияние, разрабатывать меры защиты и оценивать устойчивость системы.  

---

## Задание  

1. **Моделирование**  
   - Создать абстрактный класс `Risk`, содержащий общие свойства (идентификатор, название, описание, вероятность, влияние).  
   - Создать наследники `TechnicalRisk`, `BusinessRisk`, `SecurityRisk`, каждый из которых добавляет специфические атрибуты.  
   - Создать класс `MitigationMeasure` (мера защиты) с полями: название, тип, стоимость, эффективность.  
   - Создать класс `RiskAssessment`, который хранит список рисков и список мер защиты, а также методы расчёта общей оценки риска и устойчивости.  

2. **Алгоритм**  
   - Для каждого риска вычислить **оценку риска** = вероятность × влияние.  
   - Для каждого риска применить одну или несколько мер защиты, уменьшающих вероятность и/или влияние.  
   - Вычислить **общую устойчивость** системы как обратную величину суммарной оценки риска после применения мер.  

3. **Ввод/Вывод**  
   - Приложение читает данные из текстового файла `risks.txt` (формат, описанный ниже).  
   - После обработки выводит в консоль таблицу с оценками рисков до и после мер, а также итоговую устойчивость.  

4. **Пользовательский интерфейс**  
   - В консоли пользователь может добавить новый риск, добавить меру защиты, применить меры к риску, просмотреть отчёт.  

---

## Формат входных данных (`risks.txt`)  

Каждая строка описывает один риск в формате:  

```
<тип>;<идентификатор>;<название>;<описание>;<вероятность>;<влияние>;<специфический атрибут>
```

* `<тип>` – `TECH`, `BUS`, `SEC`  
* `<вероятность>` и `<влияние>` – числа от 0 до 1  
* `<специфический атрибут>` зависит от типа:  
  * `TECH` – `срок_проектирования` (дней)  
  * `BUS` – `потенциальный_потеря` (в рублях)  
  * `SEC` – `уровень_конфиденциальности` (1‑10)  

Пример строки:  

```
SEC;R5;Нарушение конфиденциальности;Доступ к данным без разрешения;0.3;0.8;9
```

---

## Формат вывода  

Таблица в консоли должна содержать заголовки:

```
ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка
```

После таблицы вывести строку:

```
Итоговая устойчивость системы: <значение>
```

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | Методы `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |
| 2 | Класс `MitigationMeasure` | Эффективность в диапазоне 0–1 | Эффективность >1 |
| 3 | Класс `RiskAssessment` | Методы `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | Обрабатывать пропущенные поля | Пустая строка, неверный формат |
| 5 | Консольный UI | Ввод через `Scanner` | Неверный ввод (нечисло, неверный тип) |
| 6 | Расчёт устойчивости | Устойчивость = 1 / (1 + суммарная оценка риска) | Суммарная оценка = 0 (без рисков) |
| 7 | Документация | Javadoc для всех публичных классов | |

**Дополнительные требования**  
- Программа должна быть написана на Java 17+ (или другой выбранный язык, но с поддержкой OOP).  
- Код должен компилироваться без ошибок и предупреждений.  
- Весь код должен быть покрыт комментариями, объясняющими логику.  
- В проекте должна быть единая точка входа (`main`).  

---

## Критерии оценки  

1. **Корректность модели** – 30 %  
2. **Функциональность** – 30 %  
3. **Качество кода (чистота, читаемость, комментарии)** – 20 %  
4. **Обработка ошибок и крайних случаев** – 10 %  
5. **Документация и отчёт** – 10 %  

---
```
- Applied variation methods:
  - `subject_domain` (id=1, preserve=True)
  - `algorithmic_requirements` (id=4, preserve=True)
- Verification summary:
  - Variant `1591`: passed=True, score=10, issues=0
  - Variant `1592`: passed=False, score=, issues=1
  - Variant `1593`: passed=False, score=, issues=1
  - Variant `1594`: passed=True, score=10, issues=0
  - Variant `1595`: passed=False, score=, issues=1
  - Variant `1596`: passed=True, score=10, issues=0
  - Variant `1597`: passed=False, score=, issues=1
  - Variant `1598`: passed=False, score=7, issues=3
  - Variant `1599`: passed=False, score=, issues=1

#### Generated Variants

##### Variant 1 (ID: 1591)

- Title: Лабораторная работа 6: Управление рисками в библиотеке
- Fingerprint: `lab464-v1-2afa069954fc`

```markdown
# Лабораторная работа 6
## Тема: «Анализ рисков, меры защиты и устойчивость»

### Цель
Разработать объектно‑ориентированную модель системы управления рисками для **библиотеки** и реализовать её в виде консольного приложения. Задача должна показать умение анализировать потенциальные угрозы, оценивать их влияние, разрабатывать меры защиты и оценивать устойчивость системы.

### Задание
1. **Моделирование**
   - Создать абстрактный класс `Risk` с полями: `id`, `name`, `description`, `probability`, `impact`.
   - Наследники: `TechnicalRisk`, `BusinessRisk`, `SecurityRisk`, каждый добавляет специфический атрибут: `designTime` (дней), `potentialLoss` (руб.), `confidentialityLevel` (1‑10).
   - Класс `MitigationMeasure` – `name`, `type`, `cost`, `efficiency` (0–1).
   - Класс `RiskAssessment` хранит список рисков и мер, методы расчёта общей оценки риска и устойчивости.

2. **Алгоритм**
   - Оценка риска: `probability * impact`.
   - Применить меры, уменьшающие вероятность и/или влияние.
   - **Общая устойчивость** системы вычисляется по формуле  
     \[
     \text{Resilience} = \frac{1}{\,1 + \sum \text{(оценка риска после мер)}\,}
     \]
     (если сумма оценок равна 0, устойчивость считается равной 1).

3. **Ввод/Вывод**
   - Чтение из `risks.txt` (см. формат ниже).
   - Вывод таблицы:  
     `ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка`.
   - После таблицы:  
     `Итоговая устойчивость системы: <значение>`.

4. **Пользовательский интерфейс**
   - Добавление риска, меры, применение мер к риску, просмотр отчёта.

### Формат входных данных (`risks.txt`)
Каждая строка:  
`тип;id;название;описание;вероятность;влияние;специфический атрибут`

* `тип`: `TECH`, `BUS`, `SEC`
* `вероятность`, `влияние`: 0–1
* Специфический атрибут:
  * `TECH` – `design_time` (дней)
  * `BUS` – `potential_loss` (руб.)
  * `SEC` – `confidentiality_level` (1‑10)

Пример:  
```
SEC;R5;Нарушение конфиденциальности;Доступ к данным без разрешения;0.3;0.8;9
```

### Формат вывода
Таблица с заголовками, затем строка с итоговой устойчивостью.

### Требования к реализации
| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |
| 2 | `MitigationMeasure` | Эффективность 0–1 | >1 |
| 3 | `RiskAssessment` | `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | Обрабатывать пропущенные поля | Пустая строка, неверный формат |
| 5 | Консольный UI | Ввод через `Scanner` | Неверный ввод (нечисло, неверный тип) |
| 6 | Расчёт устойчивости | Устойчивость = 1 / (1 + суммарная оценка риска) | Суммарная оценка = 0 (без рисков) |
| 7 | Документация | Javadoc для всех публичных классов | |

**Дополнительные требования**  
- Программа должна быть написана на Java 17+ (или другой выбранный язык, но с поддержкой OOP).  
- Код должен компилироваться без ошибок и предупреждений.  
- Весь код должен быть покрыт комментариями, объясняющими логику.  
- В проекте должна быть единая точка входа (`main`).  

### Критерии оценки  
1. Корректность модели – 30 %  
2. Функциональность – 30 %  
3. Качество кода (чистота, читаемость, комментарии) – 20 %  
4. Обработка ошибок и крайних случаев – 10 %  
5. Документация и отчёт – 10 %  
---
```

```json
{
  "subject_domain": "Библиотека",
  "algorithmic_requirements": "хеширование"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 2 (ID: 1592)

- Title: Управление рисками в кинотеатре: модель и консольное приложение
- Fingerprint: `lab464-v2-91c9440a6b24`

```markdown
## Лабораторная работа 6  
### Тема: «Анализ рисков, меры защиты и устойчивость»

**Цель**  
Разработать объектно‑ориентированную модель системы управления рисками для кинотеатра и реализовать её в виде консольного приложения. Задача должна показать умение анализировать потенциальные угрозы, оценивать их влияние, разрабатывать меры защиты и оценивать устойчивость системы.

---

## Задание

1. **Моделирование**  
   - **Абстрактный класс `Risk`**  
     ```java
     public abstract class Risk {
         protected String id;
         protected String name;
         protected String description;
         protected double probability; // 0–1
         protected double impact;      // 0–1
         // геттеры, сеттеры, toString(), equals(), hashCode()
     }
     ```
   - **Наследники**  
     - `TechnicalRisk` – добавляет атрибут `designTime` (дней).  
     - `BusinessRisk` – добавляет атрибут `potentialLoss` (руб).  
     - `SecurityRisk` – добавляет атрибут `confidentialityLevel` (1‑10).  
   - **Класс `MitigationMeasure`**  
     ```java
     public class MitigationMeasure {
         private String name;
         private String type;
         private double cost;
         private double efficiency; // 0–1
         // геттеры, сеттеры, toString(), equals(), hashCode()
     }
     ```
   - **Класс `RiskAssessment`**  
     Хранит риски и меры в **дереве поиска** (например, `TreeMap<String, Risk>`), обеспечивает быстрый поиск, вставку и удаление по `id`.  
     Методы: `addRisk()`, `addMeasure()`, `applyMeasures()`, `calculateOverallRisk()`, `calculateStability()`.

2. **Алгоритм**  
   - Оценка риска: `riskScore = probability * impact`.  
   - Меры защиты уменьшают вероятность и/или влияние.  
   - **Общая устойчивость**:  
     ```java
     double totalRiskAfterMeasures = risks.stream()
         .mapToDouble(r -> r.getProbability() * r.getImpact())
         .sum();
     double stability = 1.0 / (1.0 + totalRiskAfterMeasures);
     ```

3. **Ввод/Вывод**  
   - Чтение из `risks.txt` (см. формат ниже).  
   - Вывод таблицы:  

     ```
     ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка
     ```
   - После таблицы:  

     ```
     Итоговая устойчивость системы: <значение>
     ```

4. **Пользовательский интерфейс**  
   - Добавление риска, меры, применение мер к риску, просмотр отчёта.  
   - Ввод через `Scanner`.  

---

## Формат входных данных (`risks.txt`)

```
<тип>;<id>;<name>;<description>;<probability>;<impact>;<специфический атрибут>
```

* `<тип>` – `TECH`, `BUS`, `SEC`  
* `<probability>` и `<impact>` – числа от 0 до 1  
* `<специфический атрибут>`  
  * `TECH` – `designTime` (дней)  
  * `BUS` – `potentialLoss` (руб)  
  * `SEC` – `confidentialityLevel` (1‑10)

**Пример строки**

```
SEC;R5;Нарушение конфиденциальности;Доступ к данным без разрешения;0.3;0.8;9
```

---

## Алгоритмическое требование

Для хранения рисков использовать **дерево** (например, `TreeMap<String, Risk>`), что обеспечивает быстрый поиск, вставку и удаление по `id`.

---

## Требования к реализации

| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |
| 2 | `MitigationMeasure` | Эффективность 0–1 | >1 |
| 3 | `RiskAssessment` | `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | Обрабатывать пропущенные поля | Пустая строка, неверный формат |
| 5 | Консольный UI | Ввод через `Scanner` | Неверный ввод (нечисло, неверный тип) |
| 6 | Расчёт устойчивости | `1 / (1 + суммарная оценка риска после мер)` | Суммарная оценка = 0 (без рисков) |
| 7 | Документация | Javadoc для всех публичных классов | |

**Дополнительные требования**  
- Программа должна быть написана на Java 17+ (или другом OOP‑языке).  
- Код должен компилироваться без ошибок и предупреждений.  
- Весь код должен быть покрыт комментариями, объясняющими логику.  
- В проекте должна быть единая точка входа (`main`).  

---

## Критерии оценки  

1. **Корректность модели** – 30 %  
2. **Функциональность** – 30 %  
3. **Качество кода (чистота, читаемость, комментарии)** – 20 %  
4. **Обработка ошибок и крайних случаев** – 10 %  
5. **Документация и отчёт** – 10 %  

---
```

```json
{
  "subject_domain": "Кинотеатр",
  "algorithmic_requirements": "дерево"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 3 (ID: 1593)

- Title: Управление рисками в магазине: объектно‑ориентированная модель и консольное приложение
- Fingerprint: `lab464-v3-15026fd448eb`

```markdown
## Цель работы  
Разработать объектно‑ориентированную модель системы управления рисками для розничного магазина и реализовать её в виде консольного приложения, демонстрирующего анализ угроз, оценку влияния, применение мер защиты и оценку устойчивости.

## Задание  

1. **Моделирование**  
   - Абстрактный класс `Risk` с полями:  
     ```java
     /** Идентификатор риска. */
     private final String id;
     /** Название риска. */
     private final String name;
     /** Описание риска. */
     private final String description;
     /** Вероятность возникновения (0–1). */
     private double probability;
     /** Влияние на бизнес (0–1). */
     private double impact;
     ```
     *Методы `toString()`, `equals(Object)`, `hashCode()` реализованы для всех классов `Risk` и его наследников.*  
   - Наследники:  
     - `TechnicalRisk` – добавляет `designTime` (дней).  
     - `BusinessRisk` – добавляет `potentialLoss` (руб.).  
     - `SecurityRisk` – добавляет `confidentialityLevel` (1–10).  
   - Класс `MitigationMeasure`  
     ```java
     /** Название меры защиты. */
     private final String name;
     /** Тип меры (например, «техническая», «организационная»). */
     private final String type;
     /** Стоимость меры (руб.). */
     private final double cost;
     /** Эффективность (0–1). */
     private final double efficiency;
     ```
     *Эффективность проверяется в диапазоне 0–1; при значении > 1 выбрасывается `IllegalArgumentException`.*  
   - Класс `RiskAssessment`  
     - Хранит `List<Risk> risks` и `List<MitigationMeasure> measures`.  
     - Методы: `addRisk(Risk)`, `addMeasure(MitigationMeasure)`, `applyMeasures()` (применяет все меры к каждому риску, уменьшая вероятность и/или влияние).  
     - `calculateOverallRisk()` – возвращает суммарную оценку риска после применения мер.  
     - `calculateResilience()` – **обновленная формула**:  
       ```java
       double totalRisk = calculateOverallRisk();
       return 1.0 / (1.0 + totalRisk);
       ```
       *При `totalRisk == 0` возвращается `1.0` (полная устойчивость).*  

2. **Алгоритм**  
   - Для каждого риска вычисляем **оценку риска** = `probability × impact`.  
   - Меры защиты применяются в порядке добавления (стек). Каждая мера уменьшает вероятность и/или влияние в обратном порядке добавления.  
   - **Общая устойчивость** = `1 / (1 + Σ(оценка риска после мер))`.  

3. **Ввод/Вывод**  
   - Чтение из `risks.txt` в формате:  
     ```
     <тип>;<id>;<name>;<description>;<probability>;<impact>;<специфический атрибут>
     ```  
     *Обработка ошибок:*  
     - Если строка пустая или содержит меньше 7 полей – она пропускается, выводится предупреждение в консоль.  
     - Если тип не соответствует `TECH`, `BUS`, `SEC` – строка пропускается.  
     - Если числовые поля не могут быть распарсены – строка пропускается.  
   - Вывод таблицы:  
     ```
     ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка
     ```  
     После таблицы строка:  
     ```
     Итоговая устойчивость системы: <значение>
     ```  

4. **Пользовательский интерфейс**  
   - В консоли пользователь может:  
     1. Добавить новый риск.  
     2. Добавить меру защиты.  
     3. Применить меры к выбранному риску (используется стек).  
     4. Просмотреть отчёт (таблица + устойчивость).  

## Требования к реализации  

| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | `toString()`, `equals()`, `hashCode()` реализованы | Нулевые вероятность/влияние |
| 2 | `MitigationMeasure` | Эффективность 0–1 | Эффективность > 1 |
| 3 | `RiskAssessment` | Методы `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | Обрабатывать пропущенные поля | Пустая строка, неверный формат |
| 5 | Консольный UI | Ввод через `Scanner` | Неверный ввод (нечисло, неверный тип) |
| 6 | Расчёт устойчивости | `1 / (1 + суммарная оценка риска)` | Суммарная оценка = 0 (без рисков) |
| 7 | Документация | Javadoc для всех публичных классов | — |

**Дополнительные требования**  
- Программа должна быть написана на Java 17+ (или другом OOP‑поддерживающем языке).  
- Код компилируется без ошибок и предупреждений.  
- Весь код покрыт комментариями, объясняющими логику.  
- В проекте одна точка входа (`main`).  

## Критерии оценки  

1. Корректность модели – 30 %  
2. Функциональность – 30 %  
3. Качество кода (чистота, читаемость, комментарии) – 20 %  
4. Обработка ошибок и крайних случаев – 10 %  
5. Документация и отчёт – 10 %  

---
```

```json
{
  "subject_domain": "Магазин",
  "algorithmic_requirements": "стек"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 4 (ID: 1594)

- Title: Управление рисками в больнице: бинарный поиск
- Fingerprint: `lab464-v4-70936c88e9b8`

```markdown
# Лабораторная работа 6  
## Тема: «Анализ рисков, меры защиты и устойчивость»  

---

## Цель работы  
Разработать объектно‑ориентированную модель системы управления рисками для больницы, реализовать её в виде консольного приложения. Задача должна показать умение анализировать потенциальные угрозы, оценивать их влияние, разрабатывать меры защиты и оценивать устойчивость системы.  

---

## Задание  

1. **Моделирование**  
   - Создать абстрактный класс `Risk` с полями: `id`, `name`, `description`, `probability`, `impact`.  
   - Создать наследники `TechnicalRisk`, `BusinessRisk`, `SecurityRisk`, каждый из которых добавляет специфический атрибут:  
     * `TechnicalRisk` – `designDuration` (дней).  
     * `BusinessRisk` – `potentialLoss` (руб.).  
     * `SecurityRisk` – `confidentialityLevel` (1‑10).  
   - Создать класс `MitigationMeasure` с полями: `name`, `type`, `cost`, `efficiency`.  
   - Создать класс `RiskAssessment`, который хранит список рисков и список мер защиты, а также методы расчёта общей оценки риска и устойчивости.  

2. **Алгоритм**  
   - Для каждого риска вычислить **оценку риска** = `probability × impact`.  
   - Применить меры защиты, уменьшающие вероятность и/или влияние.  
   - Вычислить **общую устойчивость** системы как обратную величину суммарной оценки риска после применения мер:  
     \[
     \text{устойчивость} = \frac{1}{1 + \text{суммарная оценка риска}}
     \]  
     Если суммарная оценка риска равна 0, устойчивость считается равной 1 (полностью защищённая система).  
   - Для поиска риска по `id` можно использовать **бинарный поиск** (риски сортируются по `id`).  

3. **Ввод/Вывод**  
   - Чтение из файла `risks.txt` (формат описан ниже).  
   - После обработки вывести в консоль таблицу с оценками рисков до и после мер, а также итоговую устойчивость.  

4. **Пользовательский интерфейс**  
   - В консоли пользователь может:  
     * Добавить новый риск.  
     * Добавить меру защиты.  
     * Применить меры к риску.  
     * Просмотреть отчёт.  

---

## Формат входных данных (`risks.txt`)  

Каждая строка описывает один риск в формате:  

```
<тип>;<идентификатор>;<название>;<описание>;<вероятность>;<влияние>;<специфический атрибут>
```

* `<тип>` – `TECH`, `BUS`, `SEC`  
* `<вероятность>` и `<влияние>` – числа от 0 до 1  
* `<специфический атрибут>` зависит от типа:  
  * `TECH` – `срок_проектирования` (дней)  
  * `BUS` – `потенциальный_потеря` (в рублях)  
  * `SEC` – `уровень_конфиденциальности` (1‑10)  

Пример строки:  

```
SEC;R5;Нарушение конфиденциальности;Доступ к данным без разрешения;0.3;0.8;9
```

---

## Формат вывода  

Таблица в консоли должна содержать заголовки:

```
ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка
```

После таблицы вывести строку:

```
Итоговая устойчивость системы: <значение>
```

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | Методы `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |
| 2 | Класс `MitigationMeasure` | Эффективность в диапазоне 0–1 | Эффективность >1 |
| 3 | Класс `RiskAssessment` | Методы `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | Обрабатывать пропущенные поля, неверный формат | Пустая строка, неверный формат |
| 5 | Консольный UI | Ввод через `Scanner` | Неверный ввод (нечисло, неверный тип) |
| 6 | Расчёт устойчивости | Устойчивость = 1 / (1 + суммарная оценка риска) | Суммарная оценка = 0 (без рисков) |
| 7 | Документация | Javadoc для всех публичных классов и методов | |

**Дополнительные требования**  
- Программа должна быть написана на Java 17+ (или другом выбранном языке с поддержкой OOP).  
- Код должен компилироваться без ошибок и предупреждений.  
- Весь код должен быть покрыт комментариями, объясняющими логику.  
- В проекте должна быть единая точка входа (`main`).  

---

## Критерии оценки  

1. **Корректность модели** – 30 %  
2. **Функциональность** – 30 %  
3. **Качество кода (чистота, читаемость, комментарии)** – 20 %  
4. **Обработка ошибок и крайних случаев** – 10 %  
5. **Документация и отчёт** – 10 %  

---
```

```json
{
  "subject_domain": "Больница",
  "algorithmic_requirements": "бинарный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 5 (ID: 1595)

- Title: Управление рисками в геометрических фигурах: линейный поиск
- Fingerprint: `lab464-v5-113bfa43db49`

```markdown
## Лабораторная работа 6
### Тема: «Анализ рисков, меры защиты и устойчивость»

#### Цель работы  
Разработать объектно‑ориентированную модель системы управления рисками и реализовать её в виде консольного приложения. Задача должна показать умение анализировать потенциальные угрозы, оценивать их влияние, разрабатывать меры защиты и оценивать устойчивость системы.

#### Задание  

1. **Моделирование**  
   - Создать абстрактный класс `Risk` с полями: `id`, `name`, `description`, `probability`, `impact`.  
   - Создать наследники `TechnicalRisk`, `BusinessRisk`, `SecurityRisk`, каждый из которых добавляет специфический атрибут:  
     * `TechnicalRisk` – `designDuration` (дней)  
     * `BusinessRisk` – `potentialLoss` (руб.)  
     * `SecurityRisk` – `confidentialityLevel` (1‑10)  
   - Создать класс `MitigationMeasure` с полями: `name`, `type`, `cost`, `efficiency` (0‑1).  
   - Создать класс `RiskAssessment` с коллекциями рисков и мер, а также методами:  
     * `addRisk(Risk risk)`  
     * `addMeasure(MitigationMeasure measure)`  
     * `applyMeasures()` – применяет все меры к соответствующим рискам, уменьшая вероятность и/или влияние в соответствии с их `efficiency`  
     * `calculateOverallRisk()` – возвращает суммарную оценку риска после применения мер  
     * `calculateResilience()` – возвращает устойчивость системы по формуле  
       \[
       \text{Resilience} = \frac{1}{1 + \text{суммарная оценка риска}}
       \]  

2. **Алгоритм**  
   - Для каждого риска вычислить **оценку риска** = `probability × impact`.  
   - После применения мер вычислить **общую устойчивость** системы как обратную величину суммы оценок риска после мер (см. формулу выше).  

3. **Ввод/Вывод**  
   - Приложение читает данные из текстового файла `risks.txt` (формат, описанный ниже).  
   - После обработки выводит в консоль таблицу с оценками рисков до и после мер, а также итоговую устойчивость.  

4. **Пользовательский интерфейс**  
   - В консоли пользователь может:  
     * Добавить новый риск.  
     * Добавить меру защиты.  
     * Применить меры к конкретному риску (по `id`).  
     * Просмотреть отчёт.  

5. **Формат входных данных (`risks.txt`)**  
   ```text
   <тип>;<id>;<название>;<описание>;<вероятность>;<влияние>;<специфический атрибут>
   ```
   * `<тип>` – `TECH`, `BUS`, `SEC`  
   * `<вероятность>` и `<влияние>` – числа от 0 до 1  
   * `<специфический атрибут>`:  
     * `TECH` – `designDuration` (дней)  
     * `BUS` – `potentialLoss` (руб.)  
     * `SEC` – `confidentialityLevel` (1‑10)  

   **Обработка ошибок**  
   - Приложение должно корректно обрабатывать пропущенные поля, пустые строки и неверный формат.  
   - При обнаружении ошибки в строке выводится информативное сообщение, а строка пропускается.  

6. **Требования к реализации**  

| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | Методы `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |
| 2 | `MitigationMeasure` | Эффективность 0‑1 | Эффективность >1 |
| 3 | `RiskAssessment` | Методы `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | Обрабатывать пропущенные поля | Пустая строка, неверный формат |
| 5 | Консольный UI | Ввод через `Scanner` | Неверный ввод (нечисло, неверный тип) |
| 6 | Расчёт устойчивости | Устойчивость = 1 / (1 + суммарная оценка риска) | Суммарная оценка = 0 (без рисков) |
| 7 | Документация | Javadoc для всех публичных классов | — |

**Дополнительные требования**  
- Программа должна быть написана на Java 17+ (или другом языке с поддержкой OOP).  
- Код должен компилироваться без ошибок и предупреждений.  
- Весь код должен быть покрыт комментариями, объясняющими логику.  
- В проекте должна быть единая точка входа (`main`).  

#### Критерии оценки  

1. Корректность модели – 30 %  
2. Функциональность – 30 %  
3. Качество кода (чистота, читаемость, комментарии) – 20 %  
4. Обработка ошибок и крайних случаев – 10 %  
5. Документация и отчёт – 10 %  

---
```

```json
{
  "subject_domain": "Геометрические фигуры",
  "algorithmic_requirements": "линейный поиск"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 6 (ID: 1596)

- Title: Управление рисками в пекарне: модель и консольное приложение
- Fingerprint: `lab464-v6-f3c0fdc79267`

```markdown
# Лабораторная работа 6  
## Тема: «Анализ рисков, меры защиты и устойчивость»  

---

## Цель работы  
Разработать объектно‑ориентированную модель системы управления рисками и реализовать её в виде консольного приложения. Задача должна показать умение анализировать потенциальные угрозы, оценивать их влияние, разрабатывать меры защиты и оценивать устойчивость системы.  

---

## Задание  

1. **Моделирование**  
   - Создать абстрактный класс `Risk`, содержащий общие свойства: идентификатор, название, описание, вероятность, влияние.  
   - Создать наследники `TechnicalRisk`, `BusinessRisk`, `SecurityRisk`, каждый из которых добавляет специфические атрибуты.  
   - Создать класс `MitigationMeasure` (мера защиты) с полями: название, тип, стоимость, эффективность.  
   - Создать класс `RiskAssessment`, который хранит список рисков и список мер защиты, а также методы расчёта общей оценки риска и устойчивости.  

2. **Алгоритм**  
   - Для каждого риска вычислить **оценку риска** = вероятность × влияние.  
   - Для каждого риска применить одну или несколько мер защиты, уменьшающих вероятность и/или влияние.  
   - Вычислить **общую устойчивость** системы как обратную величину суммарной оценки риска после применения мер:  
     \[
     \text{Устойчивость} = \frac{1}{1+\sum \text{Оценка риска после мер}}
     \]
     Если суммарная оценка равна 0 (нет рисков), устойчивость считается равной 1.  

3. **Ввод/Вывод**  
   - Приложение читает данные из текстового файла `risks.txt` (формат, описанный ниже).  
   - После обработки выводит в консоль таблицу с оценками рисков до и после мер, а также итоговую устойчивость.  

4. **Пользовательский интерфейс**  
   - В консоли пользователь может:  
     1. Добавить новый риск.  
     2. Добавить меру защиты.  
     3. Применить меры к риску.  
     4. Просмотреть отчёт.  

---

## Формат входных данных (`risks.txt`)  

Каждая строка описывает один риск в формате:  

```
<тип>;<идентификатор>;<название>;<описание>;<вероятность>;<влияние>;<специфический атрибут>
```

* `<тип>` – `TECH`, `BUS`, `SEC`  
* `<вероятность>` и `<влияние>` – числа от 0 до 1  
* `<специфический атрибут>` зависит от типа:  
  * `TECH` – `срок_проектирования` (дней)  
  * `BUS` – `потенциальный_потеря` (в рублях)  
  * `SEC` – `уровень_конфиденциальности` (1‑10)  

**Пример строки:**  

```
SEC;R5;Нарушение конфиденциальности;Доступ к данным без разрешения;0.3;0.8;9
```

---

## Формат вывода  

Таблица в консоли должна содержать заголовки:

```
ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка
```

После таблицы вывести строку:

```
Итоговая устойчивость системы: <значение>
```

---

## Требования к реализации  

| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | Методы `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |
| 2 | Класс `MitigationMeasure` | Эффективность в диапазоне 0–1 | Эффективность > 1 |
| 3 | Класс `RiskAssessment` | Методы `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | Обрабатывать пропущенные поля | Пустая строка, неверный формат |
| 5 | Консольный UI | Ввод через `Scanner` | Неверный ввод (нечисло, неверный тип) |
| 6 | Расчёт устойчивости | Устойчивость = 1 / (1 + Σ Оценка риска после мер) | Суммарная оценка = 0 (без рисков) |
| 7 | Документация | Javadoc для всех публичных классов | — |

**Дополнительные требования**  
- Программа должна быть написана на Java 17+ (или другом языке с поддержкой ООП).  
- Код должен компилироваться без ошибок и предупреждений.  
- Весь код должен быть покрыт комментариями, объясняющими логику.  
- В проекте должна быть единая точка входа (`main`).  

---

## Пример рисков для пекарни  

| Тип | ID | Название | Вероятность | Влияние |
|-----|----|----------|-------------|---------|
| TECH | R1 | Отказ печи | 0.25 | 0.9 |
| BUS  | R2 | Недостаток муки | 0.40 | 0.7 |
| SEC  | R3 | Утечка рецептов | 0.15 | 0.6 |

---

## Алгоритм расчёта устойчивости  

```java
double sumRiskAfter = 0.0;
for (Risk risk : risks) {
    double riskRisk = risk.getProbability() * risk.getImpact();
    for (MitigationMeasure m : risk.getAppliedMeasures()) {
        riskRisk *= (1 - m.getEfficiency());
    }
    sumRiskAfter += riskRisk;
}
double resilience;
if (sumRiskAfter == 0.0) {
    resilience = 1.0;
} else {
    resilience = 1.0 / (1.0 + sumRiskAfter);
}
```

---

## Итоговый отчёт  

После выполнения программы пользователь увидит таблицу с оценками рисков до и после применения мер, а также итоговую устойчивость системы. Это позволит менеджеру быстро оценить эффективность мер защиты и при необходимости скорректировать стратегию управления рисками.
```

```json
{
  "subject_domain": "Пекарня",
  "algorithmic_requirements": "сортировка вставками"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 7 (ID: 1597)

- Title: Управление рисками в музее: консольное приложение на связном списке
- Fingerprint: `lab464-v7-4afef84c2eec`

```markdown
## Задание

1. **Моделирование**
   - Создать абстрактный класс `Risk` с полями: `id`, `name`, `description`, `probability`, `impact`.
   - Наследники: `TechnicalRisk`, `BusinessRisk`, `SecurityRisk` с дополнительными атрибутами:
     * `TechnicalRisk` – `designTime` (дней).
     * `BusinessRisk` – `potentialLoss` (руб.)
     * `SecurityRisk` – `confidentialityLevel` (1‑10).
   - Класс `MitigationMeasure` с полями: `name`, `type`, `cost`, `efficiency` (0‑1).
   - Класс `RiskAssessment` хранит `LinkedList<Risk>` и `LinkedList<MitigationMeasure>`. Методы: `addRisk()`, `addMeasure()`, `applyMeasures()`, `calculateOverallRisk()`, `calculateResilience()`.

2. **Алгоритм расчёта**
   - Оценка риска = probability × impact.
   - При применении меры вероятность и/или влияние уменьшаются: `probability *= (1‑efficiency)` и/или `impact *= (1‑efficiency)`.
   - Общая устойчивость = 1 / Σ(оценка риска после мер).

3. **Алгоритмическая реализация**
   - Все коллекции реализованы как связанный список (`java.util.LinkedList` или собственный `Node`‑структура). Это демонстрирует работу с динамическими структурами и позволяет легко вставлять/удалять элементы.
   - При чтении из файла риски добавляются в список по порядку, а меры – в отдельный список.

4. **Ввод/Вывод**
   - Файл `risks.txt` содержит строки вида: `SEC;R5;Нарушение конфиденциальности;Доступ к данным без разрешения;0.3;0.8;9`.
   - После обработки выводится таблица:
     `ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка`.
   - Далее строка: `Итоговая устойчивость системы: <значение>`.

5. **Пользовательский интерфейс**
   - Меню в консоли:
     1. Добавить новый риск.
     2. Добавить меру защиты.
     3. Применить меры к риску (выбор по ID).
     4. Показать отчёт.
   - Ввод проверяется: диапазоны вероятности/влияния 0‑1, эффективность 0‑1, корректный тип.

6. **Тестирование**
   - Пример входных данных: 5 рисков (2 технических, 1 бизнес‑, 2 безопасных) и 3 меры.
   - Ожидаемый вывод: таблица с двумя колонками оценки (до/после), итоговая устойчивость > 0.

7. **Критерии оценки**
   - Корректность расчётов.
   - Правильное использование связанного списка.
   - Обработка ошибок ввода.
   - Чистый и понятный код, соблюдение ООП‑принципов.

**Оценка времени**: 6 часов.
```

```json
{
  "subject_domain": "Музей",
  "algorithmic_requirements": "связанный список"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 8 (ID: 1598)

- Title: Управление рисками в ресторане с пузырьковой сортировкой
- Fingerprint: `lab464-v8-afcad0c04059`

```markdown
## Цель работы  
Разработать объектно‑ориентированную модель системы управления рисками для ресторана и реализовать её в виде консольного приложения, демонстрируя анализ угроз, оценку влияния и устойчивость после применения мер защиты.

## Задание  

1. **Моделирование**  
   - Абстрактный класс `Risk` с полями: `id`, `name`, `description`, `probability`, `impact`.  
   - Наследники: `TechnicalRisk`, `BusinessRisk`, `SecurityRisk` с дополнительными атрибутами (срок разработки, потенциальная потеря, уровень конфиденциальности).  
   - Класс `MitigationMeasure` (название, тип, cost, effectiveness).  
   - Класс `RiskAssessment` хранит списки рисков и мер, методы расчёта общей оценки риска и устойчивости.

2. **Алгоритм**  
   - Для каждого риска вычислить оценку риска = probability × impact.  
   - Применить меры, уменьшающие вероятность и/или влияние.  
   - Общую устойчивость считать как обратную величину суммарной оценки риска после мер.

3. **Ввод/Вывод**  
   - Чтение из `risks.txt` (формат: `<тип>;<id>;<name>;<desc>;<prob>;<impact>;<attr>`).  
   - Вывод таблицы: `ID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка`.  
   - После таблицы строка: `Итоговая устойчивость системы: <value>`.

4. **Пользовательский интерфейс**  
   - Добавление нового риска, меры, применение мер к риску, просмотр отчёта.

## Требования к реализации  

| № | Что реализовать | Ограничения | Крайние случаи |
|---|-----------------|------------|----------------|
| 1 | Абстрактный класс `Risk` и наследники | `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |
| 2 | `MitigationMeasure` | effectiveness ∈ [0,1] | >1 |
| 3 | `RiskAssessment` | `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |
| 4 | Чтение из файла | пропущенные поля | Пустая строка, неверный формат |

## Требования к сложности  
`complexity = "medium"`, `estimated_hours = 6`.

## Номер варианта: 8  
## Параметры варьирования  
- subject_domain: **Ресторан**  
- algorithmic_requirements: **(нет дополнительных требований)**
```

```json
{
  "subject_domain": "Ресторан",
  "algorithmic_requirements": "сортировка пузырьком"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```

##### Variant 9 (ID: 1599)

- Title: Управление рисками в фитнес‑центре: модель и консольное приложение
- Fingerprint: `lab464-v9-13b1b364c66b`

```markdown
## Лабораторная работа 6\n### Тема: «Анализ рисков, меры защиты и устойчивость»\n\n**Цель работы**\nРазработать объектно‑ориентированную модель системы управления рисками для фитнес‑центра и реализовать её в виде консольного приложения. Задача должна показать умение анализировать потенциальные угрозы, оценивать их влияние, разрабатывать меры защиты и оценивать устойчивость системы.\n\n**Задание**\n\n1. **Моделирование**\n   - Создать абстрактный класс `Risk` с полями: `id`, `name`, `description`, `probability`, `impact`.\n   - Создать наследники `TechnicalRisk`, `BusinessRisk`, `SecurityRisk`, каждый из которых добавляет специфические атрибуты.\n   - Создать класс `MitigationMeasure` (мера защиты) с полями: `title`, `type`, `cost`, `efficiency`.\n   - Создать класс `RiskAssessment`, который хранит список рисков и список мер защиты, а также методы расчёта общей оценки риска и устойчивости.\n\n2. **Алгоритм**\n   - Для каждого риска вычислить **оценку риска** = вероятность × влияние.\n   - Для каждого риска применить одну или несколько мер защиты, уменьшающих вероятность и/или влияние.\n   - Вычислить **общую устойчивость** системы как обратную величину суммарной оценки риска после применения мер.\n\n3. **Ввод/Вывод**\n   - Приложение читает данные из текстового файла `risks.txt` (формат, описанный ниже).\n   - После обработки выводит в консоль таблицу с оценками рисков до и после мер, а также итоговую устойчивость.\n\n4. **Пользовательский интерфейс**\n   - В консоли пользователь может добавить новый риск, добавить меру защиты, применить меры к риску, просмотреть отчёт.\n\n**Формат входных данных (`risks.txt`)**\nКаждая строка описывает один риск в формате:\n``\n<тип>;<идентификатор>;<название>;<описание>;<вероятность>;<влияние>;<специфический атрибут>\n```\n- `<тип>` – `TECH`, `BUS`, `SEC`\n- `<вероятность>` и `<влияние>` – числа от 0 до 1\n- `<специфический атрибут>` зависит от типа:\n  - `TECH` – `срок_проектирования` (дней)\n  - `BUS` – `потенциальный_потеря` (в рублях)\n  - `SEC` – `уровень_конфиденциальности` (1‑10)\n\n**Формат вывода**\nТаблица в консоли должна содержать заголовки:\n``\nID | Название | Тип | Вероятность | Влияние | Оценка риска | Меры | Итоговая оценка\n```\nПосле таблицы вывести строку:\n``\nИтоговая устойчивость системы: <значение>\n```\n\n**Требования к реализации**\n| № | Что реализовать | Ограничения | Крайние случаи |\n|---|-----------------|------------|----------------|\n| 1 | Абстрактный класс `Risk` и наследники | Методы `toString()`, `equals()`, `hashCode()` | Нулевые вероятности/влияния |\n| 2 | Класс `MitigationMeasure` | Эффективность в диапазоне 0–1 | Эффективность >1 |\n| 3 | Класс `RiskAssessment` | Методы `addRisk()`, `addMeasure()`, `applyMeasures()` | Пустой список рисков |\n| 4 | Чтение из файла | Обрабатывать пропущенные поля | Пустая строка, неверный формат |\n\n**Алгоритмические требования**\nДля сортировки рисков по оценке риска использовать **быструю сортировку (quick sort)**, чтобы продемонстрировать применение алгоритма сортировки в реальном приложении.\n\n**Оценка**\n- 30 % – корректность модели и классов.\n- 30 % – реализация алгоритма и корректность расчётов.\n- 20 % – пользовательский интерфейс и работа с файлом.\n- 20 % – качество кода, комментарии, документация.\n\n**Срок сдачи** – 14 дней с даты выдачи задания.\n\n---\n**Пример строки в `risks.txt`**\n``\nSEC;R5;Нарушение конфиденциальности;Доступ к данным без разрешения;0.3;0.8;9\n```
```

```json
{
  "subject_domain": "Фитнес‑центр",
  "algorithmic_requirements": "quick sort"
}
```

```json
{
  "complexity": "medium",
  "estimated_hours": 6
}
```


## Event Log

| UTC | Level | Stage | Message |
|---|---|---|---|
| `2026-04-04T08:18:53.5379461+00:00` | `info` | `init` | Test plan case started. |
| `2026-04-04T08:18:53.6942257+00:00` | `info` | `discipline.create` | Discipline created. |
| `2026-04-04T08:18:53.7044020+00:00` | `info` | `lab#1.create` | Creating lab. |
| `2026-04-04T08:18:53.8198928+00:00` | `info` | `lab#1.create` | Lab created. |
| `2026-04-04T08:18:53.9604983+00:00` | `info` | `lab#1.master.generate` | Master generation started. |
| `2026-04-04T08:19:38.3025040+00:00` | `info` | `lab#1.master.generate` | Job completed. |
| `2026-04-04T08:19:38.3532285+00:00` | `info` | `lab#1.master.review` | Master draft loaded. |
| `2026-04-04T08:19:38.4049792+00:00` | `info` | `lab#1.master.approve` | Master assignment approved. |
| `2026-04-04T08:19:38.6788174+00:00` | `info` | `lab#1.variation.apply` | Variation methods configured. |
| `2026-04-04T08:19:38.7099313+00:00` | `info` | `lab#1.variants.generate` | Variants generation started. |
| `2026-04-04T08:24:28.6577448+00:00` | `info` | `lab#1.variants.generate` | Job completed. |
| `2026-04-04T08:24:28.6949648+00:00` | `info` | `lab#1.verification.initial` | Verification job started. |
| `2026-04-04T08:26:13.4603217+00:00` | `info` | `lab#1.verification.initial` | Job completed. |
| `2026-04-04T08:26:13.5401428+00:00` | `info` | `lab#1.verification.initial` | Verification reports refreshed. |
| `2026-04-04T08:26:13.5444734+00:00` | `info` | `lab#1.verification.retry` | Retrying verification for failed variants. |
| `2026-04-04T08:26:57.7891435+00:00` | `info` | `lab#1.verification.retry.variant1554` | Job completed. |
| `2026-04-04T08:27:46.4534873+00:00` | `info` | `lab#1.verification.retry.variant1555` | Job completed. |
| `2026-04-04T08:28:40.8133515+00:00` | `info` | `lab#1.verification.retry.variant1557` | Job completed. |
| `2026-04-04T08:28:40.8285263+00:00` | `info` | `lab#1.verification.retry` | Verification reports refreshed. |
| `2026-04-04T08:28:40.8290652+00:00` | `info` | `lab#1.regeneration` | Regenerating extra variants due to failed verification. |
| `2026-04-04T08:29:53.3390124+00:00` | `info` | `lab#1.regeneration.generate` | Job completed. |
| `2026-04-04T08:29:53.3525410+00:00` | `info` | `lab#1.regeneration.verify` | Verification job started. |
| `2026-04-04T08:32:24.9362440+00:00` | `info` | `lab#1.regeneration.verify` | Job completed. |
| `2026-04-04T08:32:24.9583321+00:00` | `info` | `lab#1.regeneration.verify` | Verification reports refreshed. |
| `2026-04-04T08:32:24.9584916+00:00` | `info` | `lab#1.complete` | Lab cycle completed with unresolved verification issues. |
| `2026-04-04T08:32:24.9589354+00:00` | `info` | `lab#2.create` | Creating lab. |
| `2026-04-04T08:32:25.0065693+00:00` | `info` | `lab#2.create` | Lab created. |
| `2026-04-04T08:32:25.4312987+00:00` | `info` | `lab#2.master.generate` | Master generation started. |
| `2026-04-04T08:33:19.7151992+00:00` | `info` | `lab#2.master.generate` | Job completed. |
| `2026-04-04T08:33:19.7187614+00:00` | `info` | `lab#2.master.review` | Master draft loaded. |
| `2026-04-04T08:33:19.7330204+00:00` | `info` | `lab#2.master.approve` | Master assignment approved. |
| `2026-04-04T08:33:19.7666712+00:00` | `info` | `lab#2.variation.apply` | Variation methods configured. |
| `2026-04-04T08:33:19.7834191+00:00` | `info` | `lab#2.variants.generate` | Variants generation started. |
| `2026-04-04T08:34:48.2419162+00:00` | `info` | `lab#2.variants.generate` | Job completed. |
| `2026-04-04T08:34:48.2574107+00:00` | `info` | `lab#2.verification.initial` | Verification job started. |
| `2026-04-04T08:35:58.9309408+00:00` | `info` | `lab#2.verification.initial` | Job completed. |
| `2026-04-04T08:35:58.9519078+00:00` | `info` | `lab#2.verification.initial` | Verification reports refreshed. |
| `2026-04-04T08:35:58.9523048+00:00` | `info` | `lab#2.verification.retry` | Retrying verification for failed variants. |
| `2026-04-04T08:36:37.2025792+00:00` | `info` | `lab#2.verification.retry.variant1563` | Job completed. |
| `2026-04-04T08:36:37.2352355+00:00` | `info` | `lab#2.verification.retry` | Verification reports refreshed. |
| `2026-04-04T08:36:37.2355340+00:00` | `info` | `lab#2.complete` | Lab cycle completed successfully. |
| `2026-04-04T08:36:37.2357250+00:00` | `info` | `lab#3.create` | Creating lab. |
| `2026-04-04T08:36:37.2530551+00:00` | `info` | `lab#3.create` | Lab created. |
| `2026-04-04T08:36:37.2708766+00:00` | `info` | `lab#3.master.generate` | Master generation started. |
| `2026-04-04T08:37:19.4189051+00:00` | `info` | `lab#3.master.generate` | Job completed. |
| `2026-04-04T08:37:19.4242884+00:00` | `info` | `lab#3.master.review` | Master draft loaded. |
| `2026-04-04T08:37:19.4452121+00:00` | `info` | `lab#3.master.approve` | Master assignment approved. |
| `2026-04-04T08:37:19.4737686+00:00` | `info` | `lab#3.variation.apply` | Variation methods configured. |
| `2026-04-04T08:37:19.4886166+00:00` | `info` | `lab#3.variants.generate` | Variants generation started. |
| `2026-04-04T08:39:04.1209598+00:00` | `info` | `lab#3.variants.generate` | Job completed. |
| `2026-04-04T08:39:04.1403066+00:00` | `info` | `lab#3.verification.initial` | Verification job started. |
| `2026-04-04T08:40:36.9613832+00:00` | `info` | `lab#3.verification.initial` | Job completed. |
| `2026-04-04T08:40:36.9919555+00:00` | `info` | `lab#3.verification.initial` | Verification reports refreshed. |
| `2026-04-04T08:40:36.9959521+00:00` | `info` | `lab#3.verification.retry` | Retrying verification for failed variants. |
| `2026-04-04T08:41:23.2887449+00:00` | `info` | `lab#3.verification.retry.variant1570` | Job completed. |
| `2026-04-04T08:41:23.3008868+00:00` | `info` | `lab#3.verification.retry` | Verification reports refreshed. |
| `2026-04-04T08:41:23.3011843+00:00` | `info` | `lab#3.regeneration` | Regenerating extra variants due to failed verification. |
| `2026-04-04T08:42:37.6981687+00:00` | `info` | `lab#3.regeneration.generate` | Job completed. |
| `2026-04-04T08:42:37.7266354+00:00` | `info` | `lab#3.regeneration.verify` | Verification job started. |
| `2026-04-04T08:44:18.4773971+00:00` | `info` | `lab#3.regeneration.verify` | Job completed. |
| `2026-04-04T08:44:18.4884746+00:00` | `info` | `lab#3.regeneration.verify` | Verification reports refreshed. |
| `2026-04-04T08:44:18.4885801+00:00` | `info` | `lab#3.complete` | Lab cycle completed with unresolved verification issues. |
| `2026-04-04T08:44:18.4886640+00:00` | `info` | `lab#4.create` | Creating lab. |
| `2026-04-04T08:44:18.5112945+00:00` | `info` | `lab#4.create` | Lab created. |
| `2026-04-04T08:44:18.5284220+00:00` | `info` | `lab#4.master.generate` | Master generation started. |
| `2026-04-04T08:45:00.6988146+00:00` | `info` | `lab#4.master.generate` | Job completed. |
| `2026-04-04T08:45:00.7016835+00:00` | `info` | `lab#4.master.review` | Master draft loaded. |
| `2026-04-04T08:45:00.7222407+00:00` | `info` | `lab#4.master.approve` | Master assignment approved. |
| `2026-04-04T08:45:00.7573646+00:00` | `info` | `lab#4.variation.apply` | Variation methods configured. |
| `2026-04-04T08:45:00.7694958+00:00` | `info` | `lab#4.variants.generate` | Variants generation started. |
| `2026-04-04T08:46:39.4886612+00:00` | `info` | `lab#4.variants.generate` | Job completed. |
| `2026-04-04T08:46:39.5079064+00:00` | `info` | `lab#4.verification.initial` | Verification job started. |
| `2026-04-04T08:48:08.3133111+00:00` | `info` | `lab#4.verification.initial` | Job completed. |
| `2026-04-04T08:48:08.3241281+00:00` | `info` | `lab#4.verification.initial` | Verification reports refreshed. |
| `2026-04-04T08:48:08.3242676+00:00` | `info` | `lab#4.verification.retry` | Retrying verification for failed variants. |
| `2026-04-04T08:48:56.5646224+00:00` | `info` | `lab#4.verification.retry.variant1577` | Job completed. |
| `2026-04-04T08:49:40.7863065+00:00` | `info` | `lab#4.verification.retry.variant1581` | Job completed. |
| `2026-04-04T08:49:40.7948245+00:00` | `info` | `lab#4.verification.retry` | Verification reports refreshed. |
| `2026-04-04T08:49:40.7949550+00:00` | `info` | `lab#4.regeneration` | Regenerating extra variants due to failed verification. |
| `2026-04-04T08:50:29.0331385+00:00` | `info` | `lab#4.regeneration.generate` | Job completed. |
| `2026-04-04T08:50:29.0462106+00:00` | `info` | `lab#4.regeneration.verify` | Verification job started. |
| `2026-04-04T08:51:37.6165162+00:00` | `info` | `lab#4.regeneration.verify` | Job completed. |
| `2026-04-04T08:51:37.6369518+00:00` | `info` | `lab#4.regeneration.verify` | Verification reports refreshed. |
| `2026-04-04T08:51:37.6370654+00:00` | `info` | `lab#4.complete` | Lab cycle completed with unresolved verification issues. |
| `2026-04-04T08:51:37.6372271+00:00` | `info` | `lab#5.create` | Creating lab. |
| `2026-04-04T08:51:37.6674949+00:00` | `info` | `lab#5.create` | Lab created. |
| `2026-04-04T08:51:37.7122399+00:00` | `info` | `lab#5.master.generate` | Master generation started. |
| `2026-04-04T08:52:19.9006604+00:00` | `info` | `lab#5.master.generate` | Job completed. |
| `2026-04-04T08:52:19.9045271+00:00` | `info` | `lab#5.master.review` | Master draft loaded. |
| `2026-04-04T08:52:19.9156597+00:00` | `info` | `lab#5.master.approve` | Master assignment approved. |
| `2026-04-04T08:52:19.9405953+00:00` | `info` | `lab#5.variation.apply` | Variation methods configured. |
| `2026-04-04T08:52:19.9520246+00:00` | `info` | `lab#5.variants.generate` | Variants generation started. |
| `2026-04-04T08:54:22.5841946+00:00` | `info` | `lab#5.variants.generate` | Job completed. |
| `2026-04-04T08:54:22.5992985+00:00` | `info` | `lab#5.verification.initial` | Verification job started. |
| `2026-04-04T08:56:31.9655484+00:00` | `info` | `lab#5.verification.initial` | Job completed. |
| `2026-04-04T08:56:31.9770763+00:00` | `info` | `lab#5.verification.initial` | Verification reports refreshed. |
| `2026-04-04T08:56:31.9771904+00:00` | `info` | `lab#5.verification.retry` | Retrying verification for failed variants. |
| `2026-04-04T08:57:10.1883116+00:00` | `info` | `lab#5.verification.retry.variant1583` | Job completed. |
| `2026-04-04T08:57:58.4601260+00:00` | `info` | `lab#5.verification.retry.variant1584` | Job completed. |
| `2026-04-04T08:58:40.6529569+00:00` | `info` | `lab#5.verification.retry.variant1586` | Job completed. |
| `2026-04-04T08:59:24.8579020+00:00` | `info` | `lab#5.verification.retry.variant1587` | Job completed. |
| `2026-04-04T09:00:11.1550613+00:00` | `info` | `lab#5.verification.retry.variant1588` | Job completed. |
| `2026-04-04T09:00:11.1731546+00:00` | `info` | `lab#5.verification.retry` | Verification reports refreshed. |
| `2026-04-04T09:00:11.1742790+00:00` | `info` | `lab#5.regeneration` | Regenerating extra variants due to failed verification. |
| `2026-04-04T09:01:13.5165271+00:00` | `info` | `lab#5.regeneration.generate` | Job completed. |
| `2026-04-04T09:01:13.5306652+00:00` | `info` | `lab#5.regeneration.verify` | Verification job started. |
| `2026-04-04T09:03:22.4635106+00:00` | `info` | `lab#5.regeneration.verify` | Job completed. |
| `2026-04-04T09:03:22.4828811+00:00` | `info` | `lab#5.regeneration.verify` | Verification reports refreshed. |
| `2026-04-04T09:03:22.4830442+00:00` | `info` | `lab#5.complete` | Lab cycle completed with unresolved verification issues. |
| `2026-04-04T09:03:22.4831838+00:00` | `info` | `lab#6.create` | Creating lab. |
| `2026-04-04T09:03:22.4988399+00:00` | `info` | `lab#6.create` | Lab created. |
| `2026-04-04T09:03:22.8053273+00:00` | `info` | `lab#6.master.generate` | Master generation started. |
| `2026-04-04T09:04:06.9819952+00:00` | `info` | `lab#6.master.generate` | Job completed. |
| `2026-04-04T09:04:06.9853960+00:00` | `info` | `lab#6.master.review` | Master draft loaded. |
| `2026-04-04T09:04:07.0066234+00:00` | `info` | `lab#6.master.approve` | Master assignment approved. |
| `2026-04-04T09:04:07.0341522+00:00` | `info` | `lab#6.variation.apply` | Variation methods configured. |
| `2026-04-04T09:04:07.0509811+00:00` | `info` | `lab#6.variants.generate` | Variants generation started. |
| `2026-04-04T09:05:43.5330913+00:00` | `info` | `lab#6.variants.generate` | Job completed. |
| `2026-04-04T09:05:43.5567270+00:00` | `info` | `lab#6.verification.initial` | Verification job started. |
| `2026-04-04T09:07:16.4175924+00:00` | `info` | `lab#6.verification.initial` | Job completed. |
| `2026-04-04T09:07:16.4267031+00:00` | `info` | `lab#6.verification.initial` | Verification reports refreshed. |
| `2026-04-04T09:07:16.4268257+00:00` | `info` | `lab#6.verification.retry` | Retrying verification for failed variants. |
| `2026-04-04T09:08:04.8439692+00:00` | `info` | `lab#6.verification.retry.variant1591` | Job completed. |
| `2026-04-04T09:08:53.1084074+00:00` | `info` | `lab#6.verification.retry.variant1592` | Job completed. |
| `2026-04-04T09:09:47.3583654+00:00` | `info` | `lab#6.verification.retry.variant1593` | Job completed. |
| `2026-04-04T09:10:43.6556732+00:00` | `info` | `lab#6.verification.retry.variant1595` | Job completed. |
| `2026-04-04T09:11:48.0406900+00:00` | `info` | `lab#6.verification.retry.variant1596` | Job completed. |
| `2026-04-04T09:11:48.1000387+00:00` | `info` | `lab#6.verification.retry` | Verification reports refreshed. |
| `2026-04-04T09:11:48.1018283+00:00` | `info` | `lab#6.regeneration` | Regenerating extra variants due to failed verification. |
| `2026-04-04T09:13:00.6872705+00:00` | `info` | `lab#6.regeneration.generate` | Job completed. |
| `2026-04-04T09:13:00.7346874+00:00` | `info` | `lab#6.regeneration.verify` | Verification job started. |
| `2026-04-04T09:14:55.7008837+00:00` | `info` | `lab#6.regeneration.verify` | Job completed. |
| `2026-04-04T09:14:55.7557628+00:00` | `info` | `lab#6.regeneration.verify` | Verification reports refreshed. |
| `2026-04-04T09:14:55.7559337+00:00` | `info` | `lab#6.complete` | Lab cycle completed with unresolved verification issues. |
| `2026-04-04T09:14:55.7570042+00:00` | `info` | `done` | Case completed with failures. |
