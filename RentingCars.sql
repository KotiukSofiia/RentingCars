CREATE DATABASE RentingCars;
USE RentingCars;

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Phone NVARCHAR(20),
    Password NVARCHAR(255),
    Role NVARCHAR(20) CHECK (Role IN ('client', 'admin')) DEFAULT 'client',
    CreatedAt DATETIME DEFAULT GETDATE()
);

INSERT INTO Users (FirstName, LastName, Email, Phone, Password, Role)
VALUES
(N'Софія', N'Котюк', 'admin@gmail.com', '+380673023435', 'password@1234', 'admin'),
(N'Іван', N'Петренко', 'ivanpetrenko@gmail.com', '+380501234567', 'pass@0000', 'client'),
(N'Марія', N'Іваненко', 'mariaivanenko@gmail.com', '+380502345678', 'hash2', 'client'),
(N'Олександр', N'Коваль', 'olexkoval@gmail.com', '+380503456789', 'hash3', 'client'),
(N'Олена', N'Шевчук', 'olenashevchuk@gmail.com', '+380504567890', 'hash4', 'client');



CREATE TABLE Cars (
    CarID INT IDENTITY(1,1) PRIMARY KEY,  -- Використовуємо IDENTITY замість AUTO_INCREMENT
    Make NVARCHAR(100),  -- Марка
    Model NVARCHAR(100),  -- Модель
    Year INT CHECK (Year > 1900 AND Year <= YEAR(GETDATE())),  -- Рік випуску
    FuelType NVARCHAR(20) CHECK (FuelType IN ('Petrol', 'Diesel', 'Electric', 'Hybrid')),  -- Тип палива
    Transmission NVARCHAR(20) CHECK (Transmission IN ('Manual', 'Automatic')),  -- Коробка передач
    BodyType NVARCHAR(20) CHECK (BodyType IN ('Sedan', 'SUV', 'Minivan', 'Crossover', 'Hatchback')),  -- Тип кузова
    Passengers INT,  -- Кількість пасажирів
    LuggageCapacity INT,  -- Кількість місць для багажу
    EngineVolume DECIMAL(3,1),  -- Об'єм двигуна
    Description NVARCHAR(500),
    PricePerDay DECIMAL(10,2),  -- Ціна за день оренди
    Status NVARCHAR(20) CHECK (Status IN ('Available', 'Rented', 'Not Available')),  -- Статус (в наявності, орендовано, недоступно)
    Location NVARCHAR(255),  -- Локація
    ImageURL NVARCHAR(255),  -- URL картинки автомобіля
    CreatedAt DATETIME DEFAULT GETDATE()  -- Дата та час створення
);

CREATE TABLE Rentals (
    RentalID INT IDENTITY(1,1) PRIMARY KEY,                                                         -- Використовуємо IDENTITY для автоінкременту
    UserID INT,                                                                                     -- Клієнт, який орендує
    CarID INT,                                                                                      -- Орендований автомобіль
    StartDate DATE,                                                                                 -- Дата початку оренди
    EndDate DATE,                                                                                   -- Дата завершення оренди
    TotalPrice DECIMAL(10,2),                                                                       -- Загальна сума оренди
    Status NVARCHAR(20) CHECK (Status IN (N'Очікується', N'Схвалено', N'Відхилено', N'Завершено')), -- Статус замовлення
    CreatedAt DATETIME DEFAULT GETDATE(),                                                           -- Дата створення
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (CarID) REFERENCES Cars(CarID)
);

CREATE TABLE Payments (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,                                                          -- Використовуємо IDENTITY для автоінкременту
    RentalID INT,                                                                                     -- Оренда, до якої належить цей платіж
    PaymentAmount DECIMAL(10,2),                                                                      -- Сума платежу
    PaymentDate DATETIME DEFAULT GETDATE(),                                                           -- Дата платежу
    PaymentStatus NVARCHAR(20) CHECK (PaymentStatus IN (N'Очікується', N'Завершено', N'Не вдалося')), -- Статус платежу
    PaymentMethod NVARCHAR(20) CHECK (PaymentMethod IN (N'Онлайн-оплата банківською картою')),        -- Метод оплати
    FOREIGN KEY (RentalID) REFERENCES Rentals(RentalID)
);

drop table Payments;
drop table Users;
drop table Rentals;
drop table Cars;

use master;
drop database RentingCars;