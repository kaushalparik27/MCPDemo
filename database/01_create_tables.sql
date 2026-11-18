-- Employee Search demo: table structure for SQL Server
-- Run this inside the MCPDemo database.

CREATE TABLE dbo.Location (
    LocationId   INT IDENTITY(1,1) PRIMARY KEY,
    City         NVARCHAR(100) NOT NULL,
    State        NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Location UNIQUE (City, State)
);

CREATE TABLE dbo.Department (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    Name         NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_Department_Name UNIQUE (Name)
);

-- Which department exists in which location
CREATE TABLE dbo.LocationDepartment (
    LocationDepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    LocationId           INT NOT NULL,
    DepartmentId         INT NOT NULL,
    CONSTRAINT FK_LocDept_Location   FOREIGN KEY (LocationId)   REFERENCES dbo.Location(LocationId),
    CONSTRAINT FK_LocDept_Department FOREIGN KEY (DepartmentId) REFERENCES dbo.Department(DepartmentId),
    CONSTRAINT UQ_LocDept UNIQUE (LocationId, DepartmentId)
);

CREATE TABLE dbo.Employee (
    EmployeeId           INT IDENTITY(1,1) PRIMARY KEY,
    FirstName            NVARCHAR(100) NOT NULL,
    LastName             NVARCHAR(100) NOT NULL,
    Email                NVARCHAR(256) NOT NULL,
    JobTitle             NVARCHAR(100) NOT NULL,
    LocationDepartmentId INT NOT NULL,
    ManagerId            INT NULL,          -- NULL = top of the company
    IsActive             BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_Employee_Email UNIQUE (Email),
    CONSTRAINT FK_Employee_LocDept FOREIGN KEY (LocationDepartmentId) REFERENCES dbo.LocationDepartment(LocationDepartmentId),
    CONSTRAINT FK_Employee_Manager FOREIGN KEY (ManagerId) REFERENCES dbo.Employee(EmployeeId),
    CONSTRAINT CK_Employee_NotOwnManager CHECK (ManagerId <> EmployeeId)
);

-- Helps search by name and finding a manager's team
CREATE INDEX IX_Employee_Name      ON dbo.Employee (LastName, FirstName);
CREATE INDEX IX_Employee_ManagerId ON dbo.Employee (ManagerId);
CREATE INDEX IX_Employee_LocDept   ON dbo.Employee (LocationDepartmentId);
