/* ============================================================
   RaceDay Database Script
   PROG6212 - Programming 2B
   PoE Part 1 - Section C: SQL Database Script

   Matches:
   - RaceDay ERD
   - RaceDay API Endpoint Plan

   Database entities:
   1. Role
   2. User
   3. Event
   4. Category
   5. Enrolment
   6. Result

   This script:
   - Drops existing tables for clean re-runs
   - Creates all required tables
   - Creates primary keys
   - Creates foreign keys
   - Applies NOT NULL, UNIQUE and DEFAULT constraints
   - Adds validation constraints
   - Inserts realistic sample data
   - Can be executed from top to bottom in SQL Server
   ============================================================ */


/* ============================================================
   1. DROP EXISTING TABLES
   ============================================================ */

IF OBJECT_ID('dbo.[Result]', 'U') IS NOT NULL
    DROP TABLE dbo.[Result];

IF OBJECT_ID('dbo.[Enrolment]', 'U') IS NOT NULL
    DROP TABLE dbo.[Enrolment];

IF OBJECT_ID('dbo.[Category]', 'U') IS NOT NULL
    DROP TABLE dbo.[Category];

IF OBJECT_ID('dbo.[Event]', 'U') IS NOT NULL
    DROP TABLE dbo.[Event];

IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL
    DROP TABLE dbo.[User];

IF OBJECT_ID('dbo.[Role]', 'U') IS NOT NULL
    DROP TABLE dbo.[Role];

GO


/* ============================================================
   2. ROLE TABLE
   ============================================================ */

CREATE TABLE dbo.[Role]
(
    RoleID INT IDENTITY(1,1) NOT NULL,
    RoleName VARCHAR(20) NOT NULL,

    CONSTRAINT PK_Role PRIMARY KEY (RoleID),

    CONSTRAINT UQ_Role_RoleName
        UNIQUE (RoleName),

    CONSTRAINT CK_Role_RoleName
        CHECK (RoleName IN ('Organiser', 'Participant'))
);

GO


/* ============================================================
   3. USER TABLE
   ============================================================ */

CREATE TABLE dbo.[User]
(
    UserID INT IDENTITY(1,1) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    ContactNumber VARCHAR(20) NULL,
    RoleID INT NOT NULL,
    DateRegistered DATETIME NOT NULL
        CONSTRAINT DF_User_DateRegistered DEFAULT GETDATE(),

    CONSTRAINT PK_User PRIMARY KEY (UserID),

    CONSTRAINT UQ_User_Email
        UNIQUE (Email),

    CONSTRAINT FK_User_Role
        FOREIGN KEY (RoleID)
        REFERENCES dbo.[Role](RoleID)
);

GO


/* ============================================================
   4. EVENT TABLE
   ============================================================ */

CREATE TABLE dbo.[Event]
(
    EventID INT IDENTITY(1,1) NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    Description VARCHAR(MAX) NULL,
    EventDate DATE NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventType VARCHAR(20) NOT NULL,
    OrganiserID INT NOT NULL,

    CONSTRAINT PK_Event PRIMARY KEY (EventID),

    CONSTRAINT CK_Event_Distance
        CHECK (Distance > 0),

    CONSTRAINT CK_Event_EventType
        CHECK (EventType IN ('Run', 'Walk', 'Cycle')),

    CONSTRAINT FK_Event_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES dbo.[User](UserID)
);

GO


/* ============================================================
   5. CATEGORY TABLE
   ============================================================ */

CREATE TABLE dbo.[Category]
(
    CategoryID INT IDENTITY(1,1) NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventID INT NOT NULL,

    CONSTRAINT PK_Category PRIMARY KEY (CategoryID),

    CONSTRAINT CK_Category_Distance
        CHECK (Distance > 0),

    CONSTRAINT UQ_Category_Event_CategoryName
        UNIQUE (EventID, CategoryName),

    CONSTRAINT UQ_Category_CategoryEvent
        UNIQUE (CategoryID, EventID),

    CONSTRAINT FK_Category_Event
        FOREIGN KEY (EventID)
        REFERENCES dbo.[Event](EventID)
);

GO


/* ============================================================
   6. ENROLMENT TABLE
   ============================================================ */

CREATE TABLE dbo.[Enrolment]
(
    EnrolmentID INT IDENTITY(1,1) NOT NULL,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL
        CONSTRAINT DF_Enrolment_Date DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL
        CONSTRAINT DF_Enrolment_Status DEFAULT 'Confirmed',

    CONSTRAINT PK_Enrolment PRIMARY KEY (EnrolmentID),

    CONSTRAINT UQ_Enrolment_ParticipantEvent
        UNIQUE (ParticipantID, EventID),

    CONSTRAINT CK_Enrolment_Status
        CHECK (Status IN ('Confirmed', 'Cancelled')),

    CONSTRAINT FK_Enrolment_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES dbo.[User](UserID),

    CONSTRAINT FK_Enrolment_Event
        FOREIGN KEY (EventID)
        REFERENCES dbo.[Event](EventID),

    CONSTRAINT FK_Enrolment_CategoryEvent
        FOREIGN KEY (CategoryID, EventID)
        REFERENCES dbo.[Category](CategoryID, EventID)
);

GO


/* ============================================================
   7. RESULT TABLE
   ============================================================ */

CREATE TABLE dbo.[Result]
(
    ResultID INT IDENTITY(1,1) NOT NULL,
    EnrolmentID INT NOT NULL,
    FinishTime TIME NULL,
    FinishPosition INT NULL,
    CapturedByID INT NOT NULL,

    CONSTRAINT PK_Result PRIMARY KEY (ResultID),

    CONSTRAINT UQ_Result_Enrolment
        UNIQUE (EnrolmentID),

    CONSTRAINT CK_Result_FinishPosition
        CHECK (FinishPosition IS NULL OR FinishPosition > 0),

    CONSTRAINT FK_Result_Enrolment
        FOREIGN KEY (EnrolmentID)
        REFERENCES dbo.[Enrolment](EnrolmentID),

    CONSTRAINT FK_Result_CapturedBy
        FOREIGN KEY (CapturedByID)
        REFERENCES dbo.[User](UserID)
);

GO


/* ============================================================
   8. SAMPLE DATA - ROLES
   ============================================================ */

INSERT INTO dbo.[Role] (RoleName)
VALUES
    ('Organiser'),
    ('Participant');

GO


/* ============================================================
   9. SAMPLE DATA - USERS
      2 Organisers
      2 Participants
   ============================================================ */

INSERT INTO dbo.[User]
(
    FullName,
    Email,
    PasswordHash,
    ContactNumber,
    RoleID
)
VALUES
(
    'Thandiwe Mokoena',
    'thandiwe.mokoena@raceday.co.za',
    '$2b$12$RaceDayDemoHashThandiwe001',
    '0821234567',
    1
),
(
    'Johan van der Merwe',
    'johan.vandermerwe@raceday.co.za',
    '$2b$12$RaceDayDemoHashJohan002',
    '0837654321',
    1
),
(
    'Lindiwe Dube',
    'lindiwe.dube@gmail.com',
    '$2b$12$RaceDayDemoHashLindiwe003',
    '0731122334',
    2
),
(
    'Sipho Ndlovu',
    'sipho.ndlovu@gmail.com',
    '$2b$12$RaceDayDemoHashSipho004',
    '0845566778',
    2
);

GO


/* ============================================================
   10. SAMPLE DATA - EVENTS
       3 Events
   ============================================================ */

INSERT INTO dbo.[Event]
(
    EventName,
    Description,
    EventDate,
    Location,
    Distance,
    EventType,
    OrganiserID
)
VALUES
(
    'Cape Town Peninsula Marathon',
    'A scenic road race along the Cape Peninsula coastline.',
    '2026-11-08',
    'Cape Town, Western Cape',
    42.20,
    'Run',
    1
),
(
    'Durban Beachfront Fun Walk',
    'A family-friendly walk along the Durban beachfront promenade.',
    '2026-10-04',
    'Durban, KwaZulu-Natal',
    10.00,
    'Walk',
    2
),
(
    'Johannesburg Hill Cycle Challenge',
    'A challenging cycling route through the northern suburbs of Johannesburg.',
    '2026-09-27',
    'Johannesburg, Gauteng',
    60.00,
    'Cycle',
    1
);

GO


/* ============================================================
   11. SAMPLE DATA - CATEGORIES
       Categories for every event
   ============================================================ */

INSERT INTO dbo.[Category]
(
    CategoryName,
    Distance,
    EventID
)
VALUES
(
    'Full Marathon',
    42.20,
    1
),
(
    'Half Marathon',
    21.10,
    1
),
(
    '5km Fun Walk',
    5.00,
    2
),
(
    '10km Fun Walk',
    10.00,
    2
),
(
    '60km Open',
    60.00,
    3
),
(
    '30km Novice',
    30.00,
    3
);

GO


/* ============================================================
   12. SAMPLE DATA - ENROLMENTS
       Participants enrolling in events and categories
   ============================================================ */

INSERT INTO dbo.[Enrolment]
(
    ParticipantID,
    EventID,
    CategoryID,
    Status
)
VALUES
(
    3,
    1,
    1,
    'Confirmed'
),
(
    4,
    1,
    2,
    'Confirmed'
),
(
    3,
    2,
    3,
    'Confirmed'
),
(
    4,
    3,
    6,
    'Confirmed'
);

GO


/* ============================================================
   13. SAMPLE DATA - RESULTS
       Results captured by Organisers
   ============================================================ */

INSERT INTO dbo.[Result]
(
    EnrolmentID,
    FinishTime,
    FinishPosition,
    CapturedByID
)
VALUES
(
    1,
    '03:45:12',
    5,
    1
),
(
    2,
    '01:52:30',
    3,
    1
),
(
    3,
    '00:58:45',
    8,
    2
),
(
    4,
    '01:47:20',
    6,
    1
);

GO


/* ============================================================
   14. VERIFICATION QUERIES
       These can be run after the main script to verify
       that the database and sample data were created.
   ============================================================ */

SELECT * FROM dbo.[Role];

SELECT * FROM dbo.[User];

SELECT * FROM dbo.[Event];

SELECT * FROM dbo.[Category];

SELECT * FROM dbo.[Enrolment];

SELECT * FROM dbo.[Result];

GO


/* ============================================================
   END OF RACE DAY DATABASE SCRIPT
   ============================================================ */