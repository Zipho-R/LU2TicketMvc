# RaceDay ERD Description

## 1. Overview

The RaceDay Event Management System uses a relational database to manage users, sporting events, event categories, participant enrolments and race results.

The Entity Relationship Diagram (ERD) represents the structure of the database and shows how the six main entities are connected.

The six entities are:

1. Role
2. User
3. Event
4. Category
5. Enrolment
6. Result

The database design uses primary keys to uniquely identify records and foreign keys to maintain relationships between related entities.

## 2. Entity Descriptions

### 2.1 Role

The Role entity stores the roles available within the RaceDay system.

The two roles are:

* Organiser
* Participant

`RoleID` is the primary key and uniquely identifies each role.

`RoleName` is unique to prevent duplicate role definitions.

### 2.2 User

The User entity stores registered RaceDay users.

A user has:

* A unique user ID
* Full name
* Email address
* Password hash
* Contact number
* Assigned role
* Registration date

`UserID` is the primary key.

`RoleID` is a foreign key referencing the Role entity.

A user can be an Organiser or Participant depending on the role assigned to the account.

### 2.3 Event

The Event entity stores sporting events managed by RaceDay.

Each event contains information such as:

* Event name
* Description
* Event date
* Location
* Distance
* Event type
* Organiser

`EventID` is the primary key.

`OrganiserID` is a foreign key referencing `User.UserID`.

Only users with the Organiser role are intended to manage events.

### 2.4 Category

The Category entity stores the categories available within an event.

Examples include:

* Full Marathon
* Half Marathon
* 5km Fun Walk
* 10km Fun Walk
* 60km Open
* 30km Novice

`CategoryID` is the primary key.

`EventID` is a foreign key referencing the Event entity.

An event can contain multiple categories, while each category belongs to one event.

### 2.5 Enrolment

The Enrolment entity records participants registering for events and selecting event categories.

An enrolment contains:

* Participant
* Event
* Category
* Enrolment date
* Enrolment status

`EnrolmentID` is the primary key.

`ParticipantID` references the User entity.

`EventID` references the Event entity.

`CategoryID` references the Category entity.

The database prevents a participant from creating duplicate enrolments for the same event.

The relationship between participants and events is many-to-many because:

* One participant can enrol in many events.
* One event can have many participants.

The Enrolment entity resolves this many-to-many relationship.

### 2.6 Result

The Result entity stores the race result associated with an enrolment.

A result contains:

* Result ID
* Enrolment
* Finish time
* Finish position
* Organiser who captured the result

`ResultID` is the primary key.

`EnrolmentID` is a unique foreign key referencing the Enrolment entity.

This ensures that an enrolment can have at most one result.

`CapturedByID` references the User entity and identifies the organiser who recorded the result.

## 3. Relationships and Cardinality

### Role to User

**Role 1 : Many User**

One role can be assigned to many users.

Each user has one assigned role.

```text
Role 1 ───────────< User
```

### User to Event

**User 1 : Many Event**

An organiser can manage multiple events.

Each event has one organiser.

```text
User (Organiser) 1 ───────────< Event
```

### Event to Category

**Event 1 : Many Category**

One event can have multiple categories.

Each category belongs to one event.

```text
Event 1 ───────────< Category
```

### User to Enrolment

**User 1 : Many Enrolment**

A participant can create multiple enrolments.

Each enrolment belongs to one participant.

```text
User (Participant) 1 ───────────< Enrolment
```

### Event to Enrolment

**Event 1 : Many Enrolment**

An event can have many participant enrolments.

Each enrolment is associated with one event.

```text
Event 1 ───────────< Enrolment
```

### Category to Enrolment

**Category 1 : Many Enrolment**

A category can be selected by many participants.

Each enrolment selects one category.

```text
Category 1 ───────────< Enrolment
```

### Enrolment to Result

**Enrolment 1 : 0..1 Result**

An enrolment may have no result before the participant completes the event.

Once a result is recorded, an enrolment can have only one result.

```text
Enrolment 1 ─────────── 0..1 Result
```

### User to Result

**User 1 : Many Result**

An organiser can capture results for many enrolments.

Each result records the organiser who captured it.

```text
User (Organiser) 1 ───────────< Result
```

## 4. Referential Integrity

Foreign keys are used to ensure that relationships between entities remain valid.

The database relationships are:

* `User.RoleID` → `Role.RoleID`
* `Event.OrganiserID` → `User.UserID`
* `Category.EventID` → `Event.EventID`
* `Enrolment.ParticipantID` → `User.UserID`
* `Enrolment.EventID` → `Event.EventID`
* `Enrolment.CategoryID` → `Category.CategoryID`
* `Result.EnrolmentID` → `Enrolment.EnrolmentID`
* `Result.CapturedByID` → `User.UserID`

A composite relationship between `Enrolment` and `Category` also ensures that the selected category belongs to the event selected in the enrolment.

## 5. Database Design Rules

The ERD is supported by database constraints that improve data integrity.

These include:

* Primary keys uniquely identify records.
* Foreign keys maintain relationships between entities.
* Email addresses must be unique.
* Role names must be unique.
* Event types are restricted to Run, Walk or Cycle.
* Enrolment status is restricted to Confirmed or Cancelled.
* Category distances must be positive.
* Finish positions must be positive when recorded.
* A participant cannot enrol in the same event more than once.
* An enrolment can have at most one result.

## 6. Summary

The RaceDay ERD provides a structured relational model for managing the main activities of the system.

The design separates users, roles, events, categories, enrolments and results while using foreign keys to connect related records.

The Enrolment entity resolves the many-to-many relationship between participants and events, while the Result entity allows completed participant results to be recorded against their enrolments.
