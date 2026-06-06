CREATE TABLE Admissions (
    Id int  NOT NULL IDENTITY,
    AdmissionDate datetime  NOT NULL,
    DischargeDate datetime  NULL,
    PatientPesel char(11)  NOT NULL,
    WardId int  NOT NULL,
    CONSTRAINT Admissions_pk PRIMARY KEY  (Id)
);

CREATE TABLE BedAssignments (
    Id int  NOT NULL IDENTITY,
    PatientPesel char(11)  NOT NULL,
    BedId int  NOT NULL,
    "From" datetime  NOT NULL,
    "To" datetime  NULL,
    CONSTRAINT BedAssignments_pk PRIMARY KEY  (Id)
);

CREATE TABLE BedTypes (
    Id int  NOT NULL IDENTITY,
    Name nvarchar(300)  NOT NULL,
    Description nvarchar(max)  NOT NULL,
    CONSTRAINT BedTypes_pk PRIMARY KEY  (Id)
);

CREATE TABLE Beds (
    Id int  NOT NULL,
    RoomId varchar(4)  NOT NULL,
    BedTypeId int  NOT NULL,
    CONSTRAINT Beds_pk PRIMARY KEY  (Id)
);

CREATE TABLE Patients (
    Pesel char(11)  NOT NULL,
    FirstName nvarchar(50)  NOT NULL,
    LastName nvarchar(100)  NOT NULL,
    Age int  NOT NULL,
    Sex bit  NOT NULL,
    CONSTRAINT Patients_pk PRIMARY KEY  (Pesel)
);

CREATE TABLE Rooms (
    Id varchar(4)  NOT NULL,
    WardId int  NOT NULL,
    HasTv bit  NOT NULL,
    CONSTRAINT Rooms_pk PRIMARY KEY  (Id)
);

CREATE TABLE Wards (
    Id int  NOT NULL IDENTITY,
    Name nvarchar(300)  NOT NULL,
    Description nvarchar(max)  NOT NULL,
    CONSTRAINT Wards_pk PRIMARY KEY  (Id)
);

ALTER TABLE Admissions ADD CONSTRAINT Admissions_Patients
    FOREIGN KEY (PatientPesel)
    REFERENCES Patients (Pesel);

ALTER TABLE Admissions ADD CONSTRAINT Admissions_Wards
    FOREIGN KEY (WardId)
    REFERENCES Wards (Id);

ALTER TABLE BedAssignments ADD CONSTRAINT BedAssignments_Beds
    FOREIGN KEY (BedId)
    REFERENCES Beds (Id);

ALTER TABLE BedAssignments ADD CONSTRAINT BedAssignments_Patients
    FOREIGN KEY (PatientPesel)
    REFERENCES Patients (Pesel);

ALTER TABLE Beds ADD CONSTRAINT Beds_BedTypes
    FOREIGN KEY (BedTypeId)
    REFERENCES BedTypes (Id);

ALTER TABLE Beds ADD CONSTRAINT Beds_Rooms
    FOREIGN KEY (RoomId)
    REFERENCES Rooms (Id);

ALTER TABLE Rooms ADD CONSTRAINT Room_Ward
    FOREIGN KEY (WardId)
    REFERENCES Wards (Id);

INSERT INTO Wards (Name, Description)
VALUES
(N'Kardiologia', N'Oddział chorób serca i układu krążenia'),
(N'Chirurgia', N'Oddział chirurgii ogólnej'),
(N'Ortopedia', N'Oddział leczenia urazów i schorzeń kości'),
(N'Pediatria', N'Oddział dziecięcy'),
(N'Neurologia', N'Oddział chorób układu nerwowego');

INSERT INTO BedTypes (Name, Description)
VALUES
(N'Standard', N'Łóżko standardowe'),
(N'Intensywna terapia', N'Łóżko OIOM'),
(N'Rehabilitacyjne', N'Łóżko rehabilitacyjne'),
(N'Dziecięce', N'Łóżko pediatryczne'),
(N'Elektryczne', N'Łóżko sterowane elektrycznie');

INSERT INTO Patients (Pesel, FirstName, LastName, Age, Sex)
VALUES
('90010112345', N'Jan', N'Kowalski', 35, 1),
('85050567890', N'Anna', N'Nowak', 40, 0),
('72031245678', N'Piotr', N'Wiśniewski', 53, 1),
('04122098765', N'Zuzanna', N'Kaczmarek', 20, 0),
('68111122233', N'Marek', N'Lewandowski', 57, 1);

INSERT INTO Rooms (Id, WardId, HasTv)
VALUES
('A101', 1, 1),
('B201', 2, 1),
('C301', 3, 0),
('D401', 4, 1),
('E501', 5, 0);

INSERT INTO Beds (Id, RoomId, BedTypeId)
VALUES
(1, 'A101', 1),
(2, 'B201', 2),
(3, 'C301', 3),
(4, 'D401', 4),
(5, 'E501', 5);

INSERT INTO Admissions (AdmissionDate, DischargeDate, PatientPesel, WardId)
VALUES
('2026-05-01 10:00:00', '2026-05-05 14:00:00', '90010112345', 1),
('2026-05-03 09:30:00', NULL,                  '85050567890', 2),
('2026-05-06 12:15:00', '2026-05-10 11:00:00', '72031245678', 3),
('2026-05-08 08:45:00', NULL,                  '04122098765', 4),
('2026-05-09 16:20:00', NULL,                  '68111122233', 5);

INSERT INTO BedAssignments (PatientPesel, BedId, "From", "To")
VALUES
('90010112345', 1, '2026-05-01 10:30:00', '2026-05-05 13:00:00'),
('85050567890', 2, '2026-05-03 10:00:00', NULL),
('72031245678', 3, '2026-05-06 12:30:00', '2026-05-10 10:30:00'),
('04122098765', 4, '2026-05-08 09:00:00', NULL),
('68111122233', 5, '2026-05-09 17:00:00', NULL);
