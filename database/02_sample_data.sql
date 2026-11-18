-- Employee Search demo: sample data
-- Run this inside the MCPDemo database, after 01_create_tables.sql.
-- It inserts 100 employees. It does nothing if data already exists, so it is safe to run twice.

SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM dbo.Employee)
BEGIN
    PRINT 'Sample data already exists. Nothing inserted.';
    RETURN;
END

-- 1. Locations
INSERT INTO dbo.Location (City, State) VALUES
    (N'Ahmedabad', N'Gujarat'),
    (N'Mumbai',    N'Maharashtra'),
    (N'Bengaluru', N'Karnataka'),
    (N'Pune',      N'Maharashtra');

-- 2. Departments
INSERT INTO dbo.Department (Name) VALUES
    (N'Finance'),
    (N'HR'),
    (N'IT'),
    (N'Sales');

-- 3. Which department exists in which location
--    (the same department can be in more than one location)
INSERT INTO dbo.LocationDepartment (LocationId, DepartmentId)
SELECT l.LocationId, d.DepartmentId
FROM (VALUES
        (N'Ahmedabad', N'Finance'),
        (N'Ahmedabad', N'IT'),
        (N'Ahmedabad', N'HR'),
        (N'Mumbai',    N'Finance'),
        (N'Mumbai',    N'Sales'),
        (N'Bengaluru', N'IT'),
        (N'Bengaluru', N'HR'),
        (N'Pune',      N'IT'),
        (N'Pune',      N'Sales')
     ) AS v(City, DeptName)
JOIN dbo.Location   l ON l.City = v.City
JOIN dbo.Department d ON d.Name = v.DeptName;

-- 4. Employees
--    Step A: the top manager (no manager above).
INSERT INTO dbo.Employee (FirstName, LastName, Email, JobTitle, LocationDepartmentId, ManagerId)
SELECT N'Rajesh', N'Mehta', N'rajesh.mehta@democorp.com', N'Chief Executive Officer', ld.LocationDepartmentId, NULL
FROM dbo.LocationDepartment ld
JOIN dbo.Location   l ON l.LocationId   = ld.LocationId
JOIN dbo.Department d ON d.DepartmentId = ld.DepartmentId
WHERE l.City = N'Ahmedabad' AND d.Name = N'Finance';

--    Step B: department heads. They report to the CEO.
INSERT INTO dbo.Employee (FirstName, LastName, Email, JobTitle, LocationDepartmentId, ManagerId)
SELECT v.FirstName, v.LastName, v.Email, v.JobTitle, ld.LocationDepartmentId,
       (SELECT EmployeeId FROM dbo.Employee WHERE Email = N'rajesh.mehta@democorp.com')
FROM (VALUES
        (N'Priya',  N'Shah',  N'priya.shah@democorp.com',  N'Finance Manager', N'Mumbai',    N'Finance'),
        (N'Amit',   N'Patel', N'amit.patel@democorp.com',  N'IT Manager',      N'Ahmedabad', N'IT'),
        (N'Sneha',  N'Iyer',  N'sneha.iyer@democorp.com',  N'HR Manager',      N'Bengaluru', N'HR'),
        (N'Vikram', N'Singh', N'vikram.singh@democorp.com', N'Sales Manager',  N'Pune',      N'Sales')
     ) AS v(FirstName, LastName, Email, JobTitle, City, DeptName)
JOIN dbo.Location   l  ON l.City = v.City
JOIN dbo.Department d  ON d.Name = v.DeptName
JOIN dbo.LocationDepartment ld ON ld.LocationId = l.LocationId AND ld.DepartmentId = d.DepartmentId;

--    Step C: team members. Each reports to a department head.
INSERT INTO dbo.Employee (FirstName, LastName, Email, JobTitle, LocationDepartmentId, ManagerId)
SELECT v.FirstName, v.LastName, v.Email, v.JobTitle, ld.LocationDepartmentId, m.EmployeeId
FROM (VALUES
        (N'Neha',   N'Desai',  N'neha.desai@democorp.com',   N'Accountant',        N'Ahmedabad', N'Finance', N'priya.shah@democorp.com'),
        (N'Rohan',  N'Joshi',  N'rohan.joshi@democorp.com',  N'Senior Developer',  N'Ahmedabad', N'IT',      N'amit.patel@democorp.com'),
        (N'Kavya',  N'Nair',   N'kavya.nair@democorp.com',   N'Software Engineer', N'Bengaluru', N'IT',      N'amit.patel@democorp.com'),
        (N'Arjun',  N'Kulkarni', N'arjun.kulkarni@democorp.com', N'Sales Executive', N'Mumbai',  N'Sales',   N'vikram.singh@democorp.com'),
        (N'Meera',  N'Reddy',  N'meera.reddy@democorp.com',  N'HR Executive',      N'Ahmedabad', N'HR',      N'sneha.iyer@democorp.com')
     ) AS v(FirstName, LastName, Email, JobTitle, City, DeptName, ManagerEmail)
JOIN dbo.Location   l  ON l.City = v.City
JOIN dbo.Department d  ON d.Name = v.DeptName
JOIN dbo.LocationDepartment ld ON ld.LocationId = l.LocationId AND ld.DepartmentId = d.DepartmentId
JOIN dbo.Employee m    ON m.Email = v.ManagerEmail;

--    Step D: 90 more team members, made from name lists (so 100 employees in total).
--    They are spread over all 9 location-department pairs and report to the
--    department head of their department.
DECLARE @First TABLE (Idx INT PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @First (Idx, Name) VALUES
    (0, N'Aarav'),  (1, N'Diya'),    (2, N'Ishaan'),  (3, N'Anaya'),   (4, N'Kabir'),
    (5, N'Tara'),   (6, N'Yash'),    (7, N'Pooja'),   (8, N'Harsh'),   (9, N'Nidhi'),
    (10, N'Manav'), (11, N'Riya'),   (12, N'Dhruv'),  (13, N'Simran'), (14, N'Karan'),
    (15, N'Ananya'),(16, N'Varun'),  (17, N'Isha'),   (18, N'Nikhil'), (19, N'Divya'),
    (20, N'Sahil'), (21, N'Pallavi'),(22, N'Tushar'), (23, N'Komal'),  (24, N'Gaurav'),
    (25, N'Shreya'),(26, N'Mihir'),  (27, N'Bhavna'), (28, N'Jay'),    (29, N'Hetal');

DECLARE @Last TABLE (Idx INT PRIMARY KEY, Name NVARCHAR(50));
INSERT INTO @Last (Idx, Name) VALUES
    (0, N'Sharma'),  (1, N'Verma'),   (2, N'Gupta'),   (3, N'Trivedi'), (4, N'Pandya'),
    (5, N'Bhatt'),   (6, N'Chauhan'), (7, N'Thakkar'), (8, N'Modi'),    (9, N'Parmar'),
    (10, N'Solanki'),(11, N'Rathod'), (12, N'Jain'),   (13, N'Agarwal'),(14, N'Kapoor'),
    (15, N'Malhotra'),(16, N'Bose'),  (17, N'Menon'),  (18, N'Pillai'), (19, N'Hegde'),
    (20, N'Shetty'), (21, N'Gokhale'),(22, N'Deshmukh'),(23, N'Patil'), (24, N'Jadhav'),
    (25, N'Pawar'),  (26, N'Shinde'), (27, N'More'),   (28, N'Kale'),   (29, N'Naik');

DECLARE @Title TABLE (DeptName NVARCHAR(100), Idx INT, Title NVARCHAR(100));
INSERT INTO @Title (DeptName, Idx, Title) VALUES
    (N'Finance', 0, N'Accountant'),       (N'Finance', 1, N'Financial Analyst'), (N'Finance', 2, N'Auditor'),
    (N'HR',      0, N'HR Executive'),     (N'HR',      1, N'Recruiter'),         (N'HR',      2, N'Payroll Specialist'),
    (N'IT',      0, N'Software Engineer'),(N'IT',      1, N'Senior Developer'),  (N'IT',      2, N'QA Engineer'),
    (N'Sales',   0, N'Sales Executive'),  (N'Sales',   1, N'Account Manager'),   (N'Sales',   2, N'Sales Associate');

DECLARE @Head TABLE (DeptName NVARCHAR(100), Email NVARCHAR(256));
INSERT INTO @Head (DeptName, Email) VALUES
    (N'Finance', N'priya.shah@democorp.com'),
    (N'IT',      N'amit.patel@democorp.com'),
    (N'HR',      N'sneha.iyer@democorp.com'),
    (N'Sales',   N'vikram.singh@democorp.com');

WITH Nums AS (
    SELECT 0 AS n
    UNION ALL
    SELECT n + 1 FROM Nums WHERE n < 89
),
LocDept AS (
    -- Number the 9 location-department pairs from 0 to 8
    SELECT LocationDepartmentId, DepartmentId,
           ROW_NUMBER() OVER (ORDER BY LocationDepartmentId) - 1 AS rn
    FROM dbo.LocationDepartment
)
INSERT INTO dbo.Employee (FirstName, LastName, Email, JobTitle, LocationDepartmentId, ManagerId)
SELECT f.Name,
       l.Name,
       LOWER(f.Name) + N'.' + LOWER(l.Name) + N'@democorp.com',
       t.Title,
       ld.LocationDepartmentId,
       mgr.EmployeeId
FROM Nums
JOIN @First f           ON f.Idx = n % 30
JOIN @Last  l           ON l.Idx = (7 * (n % 30) + n / 30) % 30   -- every first + last name pair is different
JOIN LocDept ld         ON ld.rn = n % 9
JOIN dbo.Department d   ON d.DepartmentId = ld.DepartmentId
JOIN @Title t           ON t.DeptName = d.Name AND t.Idx = (n / 9) % 3
JOIN @Head h            ON h.DeptName = d.Name
JOIN dbo.Employee mgr   ON mgr.Email = h.Email;

PRINT 'Sample data inserted.';
