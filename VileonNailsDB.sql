-- ============================================================================
-- SCRIPT DE CREACIÓN DE BASE DE DATOS Y TABLAS EN SQL SERVER
-- PROYECTO: VILEON NAILS STUDIO (SISTEMA DE CITAS & MANICURA RUSA)
-- DESARROLLADO PARA: SQL SERVER 2017+ / Azure SQL Database
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'VileonNailsDB')
BEGIN
    CREATE DATABASE VileonNailsDB;
END
GO

USE VileonNailsDB;
GO

-- ============================================================================
-- 1. LIMPIEZA DE TABLAS SI YA EXISTEN (ORDEN INVERSO DE DEPENDENCIAS)
-- ============================================================================
IF OBJECT_ID('dbo.WhatsAppNotifications', 'U') IS NOT NULL DROP TABLE dbo.WhatsAppNotifications;
IF OBJECT_ID('dbo.Appointments', 'U') IS NOT NULL DROP TABLE dbo.Appointments;
IF OBJECT_ID('dbo.NailDesigns', 'U') IS NOT NULL DROP TABLE dbo.NailDesigns;
IF OBJECT_ID('dbo.Manicurists', 'U') IS NOT NULL DROP TABLE dbo.Manicurists;
IF OBJECT_ID('dbo.Services', 'U') IS NOT NULL DROP TABLE dbo.Services;
GO

-- ============================================================================
-- 2. CREACIÓN DE TABLAS PRINCIPALES
-- ============================================================================

-- A. TABLA DE SERVICIOS
CREATE TABLE dbo.Services (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    PriceRD DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    DurationMinutes INT NOT NULL DEFAULT 60,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- B. TABLA DE MANICURISTAS
CREATE TABLE dbo.Manicurists (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(150) NOT NULL,
    Specialty NVARCHAR(150) NOT NULL,
    Rating FLOAT NOT NULL DEFAULT 5.0,
    PhotoUrl NVARCHAR(500) NULL,
    WorkingHours NVARCHAR(100) NOT NULL DEFAULT '9:00 AM - 7:00 PM',
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- C. TABLA DE GALERÍA DE DISEÑOS
CREATE TABLE dbo.NailDesigns (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    LikesCount INT NOT NULL DEFAULT 0,
    ManicuristId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_NailDesigns_Manicurists FOREIGN KEY (ManicuristId) REFERENCES dbo.Manicurists(Id) ON DELETE CASCADE
);
GO

-- D. TABLA DE CITAS
CREATE TABLE dbo.Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookingCode NVARCHAR(20) NOT NULL UNIQUE,
    ClientName NVARCHAR(150) NOT NULL,
    ClientPhone NVARCHAR(30) NOT NULL,
    ServiceId INT NOT NULL,
    ManicuristId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    TimeSlot NVARCHAR(20) NOT NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Pending', -- Pending, Confirmed, Completed, Cancelled, Rescheduled
    TotalAmountRD DECIMAL(18, 2) NOT NULL DEFAULT 0.00,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Appointments_Services FOREIGN KEY (ServiceId) REFERENCES dbo.Services(Id),
    CONSTRAINT FK_Appointments_Manicurists FOREIGN KEY (ManicuristId) REFERENCES dbo.Manicurists(Id)
);
GO

-- E. TABLA DE NOTIFICACIONES WHATSAPP
CREATE TABLE dbo.WhatsAppNotifications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Phone NVARCHAR(30) NOT NULL,
    MessageType NVARCHAR(50) NOT NULL DEFAULT 'BookingConfirmation', -- BookingConfirmation, Reminder, Cancellation
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Status NVARCHAR(30) NOT NULL DEFAULT 'Sent', -- Sent, Delivered, Read, Replied
    CONSTRAINT FK_WhatsAppNotifications_Appointments FOREIGN KEY (AppointmentId) REFERENCES dbo.Appointments(Id) ON DELETE CASCADE
);
GO

-- ============================================================================
-- 3. CREACIÓN DE ÍNDICES PARA ALTO RENDIMIENTO
-- ============================================================================
CREATE INDEX IX_Appointments_BookingCode ON dbo.Appointments(BookingCode);
CREATE INDEX IX_Appointments_ClientPhone ON dbo.Appointments(ClientPhone);
CREATE INDEX IX_Appointments_AppointmentDate ON dbo.Appointments(AppointmentDate);
CREATE INDEX IX_Appointments_Status ON dbo.Appointments(Status);
CREATE INDEX IX_NailDesigns_Category ON dbo.NailDesigns(Category);
GO

-- ============================================================================
-- 4. INSERT DE DATOS INICIALES (SEED DATA)
-- ============================================================================

-- Insertar Servicios Iniciales
INSERT INTO dbo.Services (Name, Category, Description, PriceRD, DurationMinutes, ImageUrl, IsActive)
VALUES 
('Acrílico + Gel Polish', 'Acrílicas', 'Set completo de uñas acrílicas con acabado en gel de alta duración.', 1500.00, 120, 'https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=600&q=80', 1),
('Manicura Rusa Combinada', 'Manicura Rusa', 'Limpieza profunda de cutícula con torno y esmaltado impecable bajo cutícula.', 1200.00, 90, 'https://images.unsplash.com/photo-1632345031435-8727f6897d53?auto=format&fit=crop&w=600&q=80', 1),
('Soft Gel Extensions (Tips)', 'Soft Gel', 'Extensión de uñas flexibles en gel premoldeado. Súper livianas y duraderas.', 1800.00, 90, 'https://images.unsplash.com/photo-1519014816548-bf5fe059798b?auto=format&fit=crop&w=600&q=80', 1),
('Pedicura Spa & Gel', 'Pedicura Spa', 'Exfoliación profunda, baño de sales, hidratación y esmaltado permanente.', 1400.00, 75, 'https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?auto=format&fit=crop&w=600&q=80', 1),
('Nail Art 3D & Cristales (Set Completo)', 'Nail Art', 'Diseño artístico personalizado con pedrería Swarosvki y relieves 3D.', 2200.00, 150, 'https://images.unsplash.com/photo-1599940824399-b87987ceb72a?auto=format&fit=crop&w=600&q=80', 1);

-- Insertar Manicuristas Iniciales
INSERT INTO dbo.Manicurists (FullName, Specialty, Rating, PhotoUrl, WorkingHours, IsAvailable)
VALUES
('Valeria Morales', 'Especialista en Acrílico & 3D Art', 4.9, 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=300&q=80', '9:00 AM - 7:00 PM', 1),
('Carolina Gómez', 'Master en Manicura Rusa & Soft Gel', 5.0, 'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=300&q=80', '9:00 AM - 7:00 PM', 1),
('Ana Luisa Reyes', 'Pedicura Spa & Esmaltado Permanente', 4.8, 'https://images.unsplash.com/photo-1567532939604-b6b5b0db2604?auto=format&fit=crop&w=300&q=80', '9:00 AM - 5:00 PM', 1);

-- Insertar Diseños de Uñas en Galería
INSERT INTO dbo.NailDesigns (Title, Category, ImageUrl, LikesCount, ManicuristId)
VALUES
('Francés Moderno con Hoja de Oro', 'Acrílicas', 'https://images.unsplash.com/photo-1604654894610-df63bc536371?auto=format&fit=crop&w=600&q=80', 142, 1),
('Nude Elegante Manicura Rusa', 'Manicura Rusa', 'https://images.unsplash.com/photo-1632345031435-8727f6897d53?auto=format&fit=crop&w=600&q=80', 98, 2),
('Glamour 3D con Cristales Swarosvki', 'Nail Art', 'https://images.unsplash.com/photo-1599940824399-b87987ceb72a?auto=format&fit=crop&w=600&q=80', 210, 1);

-- Insertar Citas Iniciales de Ejemplo
INSERT INTO dbo.Appointments (BookingCode, ClientName, ClientPhone, ServiceId, ManicuristId, AppointmentDate, TimeSlot, Status, TotalAmountRD, Notes)
VALUES
('VN-1001', 'María Rodríguez', '+18095550192', 1, 1, CAST(GETDATE() AS DATE), '09:00 AM', 'Confirmed', 1500.00, 'Preferencia por forma almendrada'),
('VN-1002', 'Ana Martínez', '+18095550188', 2, 2, CAST(GETDATE() AS DATE), '10:30 AM', 'Pending', 1200.00, 'Primera vez en el estudio'),
('VN-1003', 'Carolina Peña', '+18095550177', 4, 3, CAST(GETDATE() AS DATE), '12:00 PM', 'Completed', 1400.00, 'Clienta VIP'),
('VN-1004', 'Laura Torres', '+18095550144', 3, 2, DATEADD(DAY, 1, CAST(GETDATE() AS DATE)), '02:00 PM', 'Confirmed', 1800.00, '');

-- Insertar Notificación de Ejemplo
INSERT INTO dbo.WhatsAppNotifications (AppointmentId, Phone, MessageType, Content, SentAt, Status)
VALUES
(1, '+18095550192', 'BookingConfirmation', '💅 Hola, María Rodríguez. Tu cita en Vileon Nails ha sido registrada para hoy a las 09:00 AM.', GETUTCDATE(), 'Sent');
GO

-- ============================================================================
-- 5. VISTA DE RESUMEN DE DASHBOARD (KPIS)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_DashboardSummary AS
SELECT
    (SELECT COUNT(*) FROM dbo.Appointments WHERE AppointmentDate = CAST(GETDATE() AS DATE)) AS TotalAppointmentsToday,
    (SELECT COUNT(*) FROM dbo.Appointments WHERE AppointmentDate = DATEADD(DAY, 1, CAST(GETDATE() AS DATE))) AS TotalAppointmentsTomorrow,
    (SELECT ISNULL(SUM(TotalAmountRD), 0) FROM dbo.Appointments WHERE AppointmentDate = CAST(GETDATE() AS DATE) AND Status != 'Cancelled') AS TotalRevenueTodayRD,
    (SELECT COUNT(*) FROM dbo.Appointments WHERE Status = 'Confirmed') AS ConfirmedCount,
    (SELECT COUNT(*) FROM dbo.Appointments WHERE Status = 'Pending') AS PendingCount,
    (SELECT COUNT(*) FROM dbo.Appointments WHERE Status = 'Completed') AS CompletedCount,
    (SELECT COUNT(*) FROM dbo.Appointments WHERE Status = 'Cancelled') AS CancelledCount;
GO

-- ============================================================================
-- 6. VISTA DETALLADA DE CITAS (CON NOMBRES DE SERVICIOS Y MANICURISTAS)
-- ============================================================================
CREATE OR ALTER VIEW dbo.vw_AppointmentDetails AS
SELECT 
    a.Id,
    a.BookingCode,
    a.ClientName,
    a.ClientPhone,
    a.ServiceId,
    s.Name AS ServiceName,
    a.ManicuristId,
    m.FullName AS ManicuristName,
    a.AppointmentDate,
    a.TimeSlot,
    a.Status,
    a.TotalAmountRD,
    a.Notes,
    a.CreatedAt
FROM dbo.Appointments a
INNER JOIN dbo.Services s ON a.ServiceId = s.Id
INNER JOIN dbo.Manicurists m ON a.ManicuristId = m.Id;
GO
