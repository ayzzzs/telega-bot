-- =============================================
-- SQL запросы для методов IToDoRepository
-- =============================================

-- =============================================
-- GetAsync(Guid id, CancellationToken ct)
-- Получить задачу по ID
-- =============================================
SELECT 
    "Id",
    "Name",
    "UserId",
    "ListId",
    "Deadline",
    "State",
    "CreatedAt"
FROM "ToDoItem"
WHERE "Id" = 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'; -- Пример ID

-- =============================================
-- GetAllByUserIdAsync(Guid userId, CancellationToken ct)
-- Получить все задачи пользователя
-- =============================================
SELECT 
    "Id",
    "Name",
    "UserId",
    "ListId",
    "Deadline",
    "State",
    "CreatedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' -- Пример UserId
ORDER BY "CreatedAt" DESC;

-- =============================================
-- GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
-- Получить активные задачи пользователя
-- =============================================
SELECT 
    "Id",
    "Name",
    "UserId",
    "ListId",
    "Deadline",
    "State",
    "CreatedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' -- Пример UserId
  AND "State" = 0 -- 0 = Active
ORDER BY "Deadline" ASC;

-- =============================================
-- GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
-- Получить задачи пользователя по списку
-- =============================================

-- Вариант 1: Задачи из конкретного списка
SELECT 
    "Id",
    "Name",
    "UserId",
    "ListId",
    "Deadline",
    "State",
    "CreatedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' -- Пример UserId
  AND "ListId" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' -- Пример ListId
  AND "State" = 0 -- Активные
ORDER BY "Deadline" ASC;

-- Вариант 2: Задачи без списка (ListId = NULL)
SELECT 
    "Id",
    "Name",
    "UserId",
    "ListId",
    "Deadline",
    "State",
    "CreatedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' -- Пример UserId
  AND "ListId" IS NULL
  AND "State" = 0 -- Активные
ORDER BY "Deadline" ASC;

-- =============================================
-- CountActiveAsync(Guid userId, CancellationToken ct)
-- Подсчитать количество активных задач пользователя
-- =============================================
SELECT COUNT(*) as "ActiveTasksCount"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' -- Пример UserId
  AND "State" = 0; -- 0 = Active

-- =============================================
-- ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
-- Проверить существование задачи с таким именем у пользователя
-- =============================================
SELECT EXISTS (
    SELECT 1
    FROM "ToDoItem"
    WHERE "UserId" = '11111111-1111-1111-1111-111111111111' -- Пример UserId
      AND LOWER("Name") = LOWER('Купить молоко') -- Пример имени (регистронезависимый поиск)
) as "Exists";

-- =============================================
-- FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
-- Поиск задач по части названия (пример предиката)
-- =============================================
SELECT 
    "Id",
    "Name",
    "UserId",
    "ListId",
    "Deadline",
    "State",
    "CreatedAt"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' -- Пример UserId
  AND "Name" ILIKE '%презент%' -- Поиск по части названия (регистронезависимый)
ORDER BY "CreatedAt" DESC;

-- =============================================
-- Дополнительные полезные запросы
-- =============================================

-- Получить все задачи с информацией о списке и пользователе (JOIN)
SELECT 
    ti."Id",
    ti."Name" as "TaskName",
    ti."Deadline",
    ti."State",
    tl."Name" as "ListName",
    tu."Username"
FROM "ToDoItem" ti
LEFT JOIN "ToDoList" tl ON ti."ListId" = tl."Id"
INNER JOIN "ToDoUser" tu ON ti."UserId" = tu."UserId"
WHERE ti."UserId" = '11111111-1111-1111-1111-111111111111'
ORDER BY ti."Deadline" ASC;

-- Статистика по пользователю (общее, завершенные, активные)
SELECT 
    COUNT(*) as "Total",
    SUM(CASE WHEN "State" = 1 THEN 1 ELSE 0 END) as "Completed",
    SUM(CASE WHEN "State" = 0 THEN 1 ELSE 0 END) as "Active"
FROM "ToDoItem"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111';

-- Получить пользователя по TelegramUserId
SELECT 
    "UserId",
    "TelegramUserId",
    "Username",
    "CreatedAt"
FROM "ToDoUser"
WHERE "TelegramUserId" = 123456789; -- Пример TelegramUserId

-- Получить все списки пользователя
SELECT 
    "Id",
    "Name",
    "UserId",
    "CreatedAt"
FROM "ToDoList"
WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
ORDER BY "CreatedAt" DESC;

-- Получить список по ID
SELECT 
    "Id",
    "Name",
    "UserId",
    "CreatedAt"
FROM "ToDoList"
WHERE "Id" = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

-- Проверить существование списка с таким именем у пользователя
SELECT EXISTS (
    SELECT 1
    FROM "ToDoList"
    WHERE "UserId" = '11111111-1111-1111-1111-111111111111'
      AND LOWER("Name") = LOWER('Работа')
) as "Exists";