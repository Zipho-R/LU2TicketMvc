# RaceDay ERD Description

## Overview

The RaceDay Event Management System database consists of six main entities:

- Role
- User
- Event
- Category
- Enrolment
- Result

The Entity Relationship Diagram (ERD) shows how these entities are connected and how the database supports the main RaceDay system requirements.

The complete ERD is available in:

`docs/RaceDay_ERD.pdf`

## Entities

### 1. Role

The Role entity stores the different roles available in the system.

A role can be assigned to many users.

Main attributes:

- `RoleID` – Primary Key
- `RoleName` – Name of the role

The system supports two roles:

- Organiser
- Participant

### 2. User

The User entity stores registered system users.

Main attributes:

- `UserID` – Primary Key
- `FullName`
- `Email`
- `PasswordHash`
- `ContactNumber`
- `RoleID` – Foreign Key
- `DateRegistered`

Each user must have a valid role.

A user with the Organiser role can manage events and capture results, while a user with the Participant role can enrol in events and view their own results.

### 3. Event

The Event entity stores information about sporting events.

Main attributes:

- `EventID` – Primary Key
- `EventName`
- `Description`
- `EventDate`
- `Location`
- `Distance`
- `EventType`
- `OrganiserID` – Foreign Key

Each event is managed by an organiser.

An organiser can manage multiple events.

### 4. Category

The Category entity stores the different participation categories available within an event.

Main attributes:

- `CategoryID` – Primary Key
- `CategoryName`
- `Distance`
- `EventID` – Foreign Key

Each category belongs to one event.

An event can have multiple categories.

### 5. Enrolment

The Enrolment entity records participants who enrol in events and select an event category.

Main attributes:

- `EnrolmentID` – Primary Key
- `ParticipantID` – Foreign Key
- `EventID` – Foreign Key
- `CategoryID` – Foreign Key
- `EnrolmentDate`
- `Status`

Enrolment resolves the many-to-many relationship between participants and events.

A participant can enrol in multiple events, and an event can have multiple participants.

The database also ensures that the selected category belongs to the same event as the enrolment.

### 6. Result

The Result entity stores the race result associated with an enrolment.

Main attributes:

- `ResultID` – Primary Key
- `EnrolmentID` – Foreign Key
- `FinishTime`
- `FinishPosition`
- `CapturedByID` – Foreign Key

Each enrolment can have at most one result.

An organiser records or captures the result.

## Relationships

### Role to User

**One-to-Many (1:M)**

One role can be assigned to many users.

Each user belongs to one role.

### User to Event

**One-to-Many (1:M)**

An organiser can manage many events.

Each event has one organiser.

### Event to Category

**One-to-Many (1:M)**

One event can contain multiple categories.

Each category belongs to one event.

### User to Enrolment

**One-to-Many (1:M)**

A participant can have multiple enrolments.

Each enrolment belongs to one participant.

### Event to Enrolment

**One-to-Many (1:M)**

An event can have many enrolments.

Each enrolment belongs to one event.

### Category to Enrolment

**One-to-Many (1:M)**

A category can be selected by multiple participants.

Each enrolment selects one category.

### Enrolment to Result

**One-to-One (1:1)**

An enrolment can have zero or one result.

Each result belongs to one enrolment.

### User to Result

**One-to-Many (1:M)**

An organiser can capture multiple results.

Each result records the organiser who captured it.

## Many-to-Many Relationship

Participants and events have a many-to-many relationship:

**Participant ↔ Event**

This relationship is resolved through the `Enrolment` entity.

This allows:

- One participant to enrol in multiple events.
- One event to have multiple participants.
- Each enrolment to identify the selected category.