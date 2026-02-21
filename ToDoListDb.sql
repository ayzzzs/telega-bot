-- =============================================
-- Создание таблиц для ToDoList приложения
-- =============================================

-- Удаление таблиц если они существуют (для повторного выполнения скрипта)
DROP TABLE IF EXISTS "ToDoItem" CASCADE;
DROP TABLE IF EXISTS "ToDoList" CASCADE;
DROP TABLE IF EXISTS "ToDoUser" CASCADE;

-- =============================================
-- Таблица пользователей
-- =============================================
CREATE TABLE "ToDoUser" (
    "UserId" UUID PRIMARY KEY,
    "TelegramUserId" BIGINT NOT NULL,
    "Username" VARCHAR(255) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =============================================
-- Таблица списков задач
-- =============================================
CREATE TABLE "ToDoList" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(10) NOT NULL,
    "UserId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =============================================
-- Таблица задач
-- =============================================
CREATE TABLE "ToDoItem" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "UserId" UUID NOT NULL,
    "ListId" UUID NULL,
    "Deadline" TIMESTAMP NOT NULL,
    "State" INT NOT NULL DEFAULT 0, -- 0 = Active, 1 = Completed
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =============================================
-- Внешние ключи
-- =============================================

-- ToDoList -> ToDoUser
ALTER TABLE "ToDoList"
ADD CONSTRAINT "FK_ToDoList_ToDoUser"
FOREIGN KEY ("UserId")
REFERENCES "ToDoUser"("UserId")
ON DELETE CASCADE;

-- ToDoItem -> ToDoUser
ALTER TABLE "ToDoItem"
ADD CONSTRAINT "FK_ToDoItem_ToDoUser"
FOREIGN KEY ("UserId")
REFERENCES "ToDoUser"("UserId")
ON DELETE CASCADE;

-- ToDoItem -> ToDoList
ALTER TABLE "ToDoItem"
ADD CONSTRAINT "FK_ToDoItem_ToDoList"
FOREIGN KEY ("ListId")
REFERENCES "ToDoList"("Id")
ON DELETE SET NULL;

-- =============================================
-- Индексы
-- =============================================

-- Уникальный индекс для TelegramUserId
CREATE UNIQUE INDEX "IX_ToDoUser_TelegramUserId"
ON "ToDoUser"("TelegramUserId");

-- Индекс для внешнего ключа ToDoList.UserId
CREATE INDEX "IX_ToDoList_UserId"
ON "ToDoList"("UserId");

-- Индекс для внешнего ключа ToDoItem.UserId
CREATE INDEX "IX_ToDoItem_UserId"
ON "ToDoItem"("UserId");

-- Индекс для внешнего ключа ToDoItem.ListId
CREATE INDEX "IX_ToDoItem_ListId"
ON "ToDoItem"("ListId");

-- Индекс для фильтрации по состоянию задачи
CREATE INDEX "IX_ToDoItem_State"
ON "ToDoItem"("State");

-- Составной индекс для частых запросов (UserId + State)
CREATE INDEX "IX_ToDoItem_UserId_State"
ON "ToDoItem"("UserId", "State");