--    данный скрипт создает базу данных FuelStation с тремя таблицами и генерирует тестовые записи:
-- 1. виды топлива (Fuels) - 1000 штук
-- 2. список емкостей (Tanks) - 100 штук
-- 3. факты совершения операций прихода, расхода топлива (Operations) - 300000 штук


-- =================================================================
-- Создание и первоначальная настройка базы данных
-- =================================================================


USE master;
GO

-- Удаляем базу, если она уже существует, для чистого старта
IF DB_ID('FuelStation') IS NOT NULL
BEGIN
    ALTER DATABASE FuelStation SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FuelStation;
END
GO

CREATE DATABASE FuelStation;
GO

ALTER DATABASE FuelStation SET RECOVERY SIMPLE;
GO

USE FuelStation;
GO

-- =================================================================
-- Создание таблиц и связей
-- =================================================================

-- 1. Виды топлива
CREATE TABLE dbo.Fuels (
    FuelID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FuelType NVARCHAR(50),
    FuelDensity REAL -- Плотность, т/м3
);

-- 2. Емкости для хранения
CREATE TABLE dbo.Tanks (
    TankID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TankType NVARCHAR(30), -- Название/маркировка емкости
    TankVolume REAL,       -- Объем, м3
    TankWeight REAL,       -- Вес пустой емкости, т
    TankMaterial NVARCHAR(30), -- Материал
    TankPicture NVARCHAR(50)
);

-- 3. Операции прихода/расхода
CREATE TABLE dbo.Operations (
    OperationID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FuelID INT,
    TankID INT,
    Inc_Exp REAL, -- >0 Приход, <0 Расход, м3
    [Date] DATE
);
GO

-- Добавление связей между таблицами
ALTER TABLE dbo.Operations WITH CHECK ADD CONSTRAINT FK_Operations_Fuels FOREIGN KEY(FuelID)
REFERENCES dbo.Fuels (FuelID) ON DELETE CASCADE;
GO
ALTER TABLE dbo.Operations WITH CHECK ADD CONSTRAINT FK_Operations_Tanks FOREIGN KEY(TankID)
REFERENCES dbo.Tanks (TankID) ON DELETE CASCADE;
GO

-- =================================================================
-- Генерация тестовых данных 
-- =================================================================
SET NOCOUNT ON;

DECLARE
    @i INT,
    @FuelType NVARCHAR(50),
    @TankType NVARCHAR(30),
    @TankMaterial NVARCHAR(30),
    @FuelID INT,
    @TankID INT,
    @odate DATE,
    @Inc_Exp REAL,
    @RowCount INT,
    @TankVolume REAL,
    -- === Параметры генерации ===
    @NumberFuels INT = 1000,
    @NumberTanks INT = 100,
    @NumberOperations INT = 300000;

BEGIN TRAN

-- === 1. Заполнение видов топлива ===
-- Создаем таблицы с частями названий для реалистичной генерации
DECLARE @FuelPrefixes TABLE (ID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @FuelPrefixes (Name) VALUES (N'Бензин'), (N'Топливо дизельное'), (N'Керосин'), (N'Мазут'), (N'Топливо реактивное'), (N'Газойль');
DECLARE @FuelBrands TABLE (ID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @FuelBrands (Name) VALUES (N'АИ'), (N'ДТ'), (N'ТС-1'), (N'М-100'), (N'Евро'), (N'Арктика');
DECLARE @FuelSuffixes TABLE (ID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @FuelSuffixes (Name) VALUES ('-92'), ('-95'), ('-98'), ('-100'), (N'-К5'), (N'-Летнее'), (N'-Зимнее'), (N' сорт C');

DECLARE @PrefixCount INT = (SELECT COUNT(*) FROM @FuelPrefixes);
DECLARE @BrandCount INT = (SELECT COUNT(*) FROM @FuelBrands);
DECLARE @SuffixCount INT = (SELECT COUNT(*) FROM @FuelSuffixes);

SET @RowCount=1
WHILE @RowCount <= @NumberFuels
BEGIN
    DECLARE @p1 NVARCHAR(50), @p2 NVARCHAR(50), @p3 NVARCHAR(50);
    -- Выбираем случайные части названия
    SELECT @p1 = Name FROM @FuelPrefixes WHERE ID = CAST(RAND() * @PrefixCount + 1 AS INT);
    SELECT @p2 = Name FROM @FuelBrands WHERE ID = CAST(RAND() * @BrandCount + 1 AS INT);
    SELECT @p3 = Name FROM @FuelSuffixes WHERE ID = CAST(RAND() * @SuffixCount + 1 AS INT);

    -- Комбинируем части для уникальности, добавляя номер
    SET @FuelType = @p1 + ' ' + @p2 + @p3 + N' (партия ' + CAST(@RowCount AS NVARCHAR(10)) + ')';

    -- Вставляем запись с реалистичной плотностью (0.7 - 1.0 т/м3)
    INSERT INTO dbo.Fuels (FuelType, FuelDensity)
    SELECT @FuelType, 0.7 + RAND() * 0.3;

    SET @RowCount +=1
END;

-- === 2. Заполнение емкостей ===
-- Создаем таблицы с типами и материалами емкостей
DECLARE @TankTypes TABLE (ID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(20));
INSERT INTO @TankTypes (Name) VALUES (N'РВС'), (N'РГС'), (N'ЕП'), (N'ЕПП'), (N'Цистерна'), (N'Бак');
DECLARE @TankMaterialsList TABLE (ID INT IDENTITY(1,1) PRIMARY KEY, Name NVARCHAR(30));
INSERT INTO @TankMaterialsList (Name) VALUES (N'Сталь 3СП'), (N'Сталь 09Г2С'), (N'Нерж. сталь 12Х18Н10Т'), (N'Алюминий АМг5'), (N'Полиэтилен HDPE');

DECLARE @TankTypeCount INT = (SELECT COUNT(*) FROM @TankTypes);
DECLARE @TankMaterialCount INT = (SELECT COUNT(*) FROM @TankMaterialsList);

SET @RowCount=1
WHILE @RowCount <= @NumberTanks
BEGIN
    -- Генерация реалистичного названия (например, РВС-1000)
    DECLARE @TypeName NVARCHAR(20);
    SELECT @TypeName = Name FROM @TankTypes WHERE ID = CAST(RAND() * @TankTypeCount + 1 AS INT);
    SET @TankType = @TypeName + '-' + CAST(CAST(5 + RAND() * 5000 AS INT) AS NVARCHAR(10));

    -- Выбор реалистичного материала
    SELECT @TankMaterial = Name FROM @TankMaterialsList WHERE ID = CAST(RAND() * @TankMaterialCount + 1 AS INT);

    -- Генерация реалистичного объема (от 5 до 5000 м3)
    SET @TankVolume = 5 + RAND() * (5000 - 5);

    -- Генерация веса в зависимости от объема (например, вес пустой емкости ~ 8-15% от объема)
    INSERT INTO dbo.Tanks (TankType, TankVolume, TankWeight, TankMaterial)
    SELECT @TankType, @TankVolume, @TankVolume * (0.08 + RAND() * 0.07), @TankMaterial;

    SET @RowCount +=1
END;


-- === 3. Заполнение операций ===
-- Для эффективности кэшируем объемы емкостей во временной таблице, чтобы не делать лишних запросов в цикле
DECLARE @TankVolumes TABLE (RowID INT IDENTITY(1,1) PRIMARY KEY, TankID INT, TankVolume REAL);
INSERT INTO @TankVolumes (TankID, TankVolume) SELECT TankID, TankVolume FROM dbo.Tanks;

SET @RowCount=1
WHILE @RowCount <= @NumberOperations
BEGIN
    -- Дата операции за последние ~4 года
    SET @odate = dateadd(day, -RAND() * 1500, GETDATE());
    -- Случайный вид топлива
    SET @FuelID = CAST(1 + RAND() * (@NumberFuels - 1) AS INT);

    -- Выбираем случайную емкость из нашего кэша
    DECLARE @RandomRow INT = CAST(RAND() * @NumberTanks + 1 AS INT);
    SELECT @TankID = TankID, @TankVolume = TankVolume FROM @TankVolumes WHERE RowID = @RandomRow;

    -- Приход/расход - случайная величина, не превышающая 20% от объема емкости
    -- (RAND() - 0.5) * 2 дает случайное число от -1 до 1
    SET @Inc_Exp = (@TankVolume * 0.2) * (RAND() - 0.5) * 2;

    INSERT INTO dbo.Operations (FuelID, TankID, Inc_Exp, [Date])
    SELECT @FuelID, @TankID, @Inc_Exp, @odate;

    SET @RowCount +=1
END;


COMMIT TRAN
GO
-- =================================================================
-- Создание представления для удобного просмотра данных
-- =================================================================
CREATE OR ALTER VIEW [dbo].[View_AllOperations]
AS
SELECT
    O.OperationID,
    O.Inc_Exp,
    O.Date,
    F.FuelID,
    F.FuelType,
    T.TankID,
    T.TankType
FROM
    dbo.Operations AS O
INNER JOIN
    dbo.Fuels AS F ON O.FuelID = F.FuelID
INNER JOIN
    dbo.Tanks AS T ON O.TankID = T.TankID;
GO
-- =================================================================
-- Создание хранимой процедуры для выборки данных
-- =================================================================
CREATE OR ALTER PROCEDURE dbo.uspGetOperations
    @FuelID int = NULL,
    @FuelType nvarchar(50) = NULL,
    @TankID int = NULL,
    @TankType nvarchar(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        OperationID,
        Inc_Exp,
        Date,
        FuelID,
        FuelType,
        TankID,
        TankType
    FROM
        dbo.View_AllOperations
    WHERE
        (@FuelID IS NULL OR FuelID = @FuelID) AND
        (@FuelType IS NULL OR FuelType LIKE '%' + @FuelType + '%') AND
        (@TankID IS NULL OR TankID = @TankID) AND
        (@TankType IS NULL OR TankType LIKE '%' + @TankType + '%');
END;
GO

-- Проверка сгенерированных данных
SELECT TOP 10 * FROM dbo.Fuels ORDER BY FuelID DESC;
SELECT TOP 10 * FROM dbo.Tanks ORDER BY TankID DESC;
SELECT TOP 10 * FROM dbo.Operations ORDER BY OperationID DESC;
GO
-- Пример использования процедуры
EXEC dbo.uspGetOperations @FuelType = 'Бензин', @TankType = 'РВС';
GO