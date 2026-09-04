# RaceDay Data Dictionary

## Overview

This document describes the main fields used by the RaceDay Event Management System database.

The data dictionary supports the Entity Relationship Diagram and SQL database design.

## Role

| Field | Data Type | Key | Description |
|---|---|---|---|
| RoleID | INT | PK | Unique identifier for a user role. |
| RoleName | VARCHAR(20) | UNIQUE | Name of the user role, such as Organiser or Participant. |

## User

| Field | Data Type | Key | Description |
|---|---|---|---|
| UserID | INT | PK | Unique identifier for a registered user. |
| FullName | VARCHAR(100) | | Full name of the user. |
| Email | VARCHAR(150) | UNIQUE | User's unique email address. |
| PasswordHash | VARCHAR(255) | | Hashed password used for authentication. |
| ContactNumber | VARCHAR(20) | | User's contact number. |
| RoleID | INT | FK | Identifies the role assigned to the user. |
| DateRegistered | DATETIME | | Date and time when the user registered. |

## Event

| Field | Data Type | Key | Description |
|---|---|---|---|
| EventID | INT | PK | Unique identifier for an event. |
| EventName | VARCHAR(150) | | Name of the sporting event. |
| Description | VARCHAR(MAX) | | Description of the event. |
| EventDate | DATE | | Date on which the event takes place. |
| Location | VARCHAR(150) | | Physical location of the event. |
| Distance | DECIMAL(5,2) | | Main distance associated with the event. |
| EventType | VARCHAR(20) | | Type of event, such as Run, Walk or Cycle. |
| OrganiserID | INT | FK | Identifies the user responsible for managing the event. |

## Category

| Field | Data Type | Key | Description |
|---|---|---|---|
| CategoryID | INT | PK | Unique identifier for an event category. |
| CategoryName | VARCHAR(100) | | Name of the category. |
| Distance | DECIMAL(5,2) | | Distance associated with the category. |
| EventID | INT | FK | Identifies the event to which the category belongs. |

## Enrolment

| Field | Data Type | Key | Description |
|---|---|---|---|
| EnrolmentID | INT | PK | Unique identifier for an enrolment. |
| ParticipantID | INT | FK | Identifies the participant who enrolled. |
| EventID | INT | FK | Identifies the event in which the participant enrolled. |
| CategoryID | INT | FK | Identifies the selected event category. |
| EnrolmentDate | DATETIME | | Date and time when the enrolment was created. |
| Status | VARCHAR(20) | | Current enrolment status, such as Confirmed or Cancelled. |

## Result

| Field | Data Type | Key | Description |
|---|---|---|---|
| ResultID | INT | PK | Unique identifier for a race result. |
| EnrolmentID | INT | FK, UNIQUE | Identifies the enrolment associated with the result. |
| FinishTime | TIME | | Participant's recorded finishing time. |
| FinishPosition | INT | | Participant's finishing position. |
| CapturedByID | INT | FK | Identifies the organiser who captured the result. |

## Key Relationships

- `User.RoleID` references `Role.RoleID`.
- `Event.OrganiserID` references `User.UserID`.
- `Category.EventID` references `Event.EventID`.
- `Enrolment.ParticipantID` references `User.UserID`.
- `Enrolment.EventID` references `Event.EventID`.
- `Enrolment.CategoryID` references `Category.CategoryID`.
- `Result.EnrolmentID` references `Enrolment.EnrolmentID`.
- `Result.CapturedByID` references `User.UserID`.

## Integrity Rules

The database uses primary and foreign keys to maintain referential integrity.

Additional constraints ensure that:

- Email addresses are unique.
- Role names are valid and unique.
- Event types are restricted to valid values.
- Enrolment statuses are restricted to valid values.
- Category distances must be positive.
- Finish positions must be positive when recorded.
- A participant cannot create duplicate enrolments for the same event.
- A category selected for an enrolment must belong to the selected event.
- An enrolment can have at most one result.