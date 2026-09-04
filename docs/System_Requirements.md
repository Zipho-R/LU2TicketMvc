# RaceDay System Requirements

## 1. Overview

The RaceDay Event Management System is designed to manage sporting events, event categories, participant enrolments and race results.

The system supports two main user roles:

- Organiser
- Participant

## 2. Functional Requirements

### FR1 — User Registration

The system shall allow users to register as either an Organiser or Participant.

### FR2 — User Authentication

The system shall authenticate registered users using their email address and password.

### FR3 — User Profile

Authenticated users shall be able to view and update their own profile information.

### FR4 — Event Management

Organisers shall be able to create, view, update and delete events that they manage.

### FR5 — Event Categories

Organisers shall be able to create, view, update and delete categories associated with their events.

### FR6 — Event Browsing

Users shall be able to view available events and their details.

### FR7 — Participant Enrolment

Participants shall be able to enrol in an event by selecting an available category.

### FR8 — Enrolment Management

Participants shall be able to view and cancel their own enrolments.

### FR9 — Enrolment Administration

Organisers shall be able to view participant enrolments for events that they manage.

### FR10 — Result Capture

Organisers shall be able to record race results for enrolled participants.

### FR11 — Result Correction

Organisers shall be able to update or correct race results that they have permission to manage.

### FR12 — Result Viewing

Participants shall be able to view their own race result history.

### FR13 — Event Results

Organisers shall be able to view results associated with events they manage.

## 3. Role Requirements

### Organiser

An Organiser shall be able to:

- Manage their user profile.
- Create and manage events.
- Create and manage event categories.
- View participant enrolments for their events.
- Capture race results.
- Update race results.
- View results for their managed events.

### Participant

A Participant shall be able to:

- Manage their user profile.
- Browse available events.
- View event categories.
- Enrol in events.
- View their enrolments.
- Cancel their own enrolments.
- View their own results.

## 4. Data Requirements

The system shall maintain data for:

- User roles
- Registered users
- Sporting events
- Event categories
- Participant enrolments
- Race results

## 5. Security Requirements

The system shall:

- Require authentication for protected operations.
- Apply role-based access control.
- Prevent participants from managing organiser resources.
- Prevent users from accessing other users' private profile or result information.
- Restrict organisers to resources they are authorised to manage.
- Store passwords as password hashes rather than plain text.

## 6. Data Integrity Requirements

The database shall:

- Use primary keys to uniquely identify records.
- Use foreign keys to maintain relationships.
- Prevent duplicate user email addresses.
- Prevent duplicate participant enrolments for the same event.
- Ensure an enrolment can have at most one result.
- Ensure selected categories belong to the selected event.
- Restrict event types to valid values.
- Restrict enrolment statuses to valid values.

## 7. Non-Functional Requirements

### Usability

The system should provide clear and understandable workflows for Organisers and Participants.

### Reliability

The database should maintain consistent and accurate records through appropriate constraints and relationships.

### Security

Access to protected operations should be controlled using authentication and authorisation.

### Maintainability

The system should use a structured relational database and clearly documented API design to support future development.

### Scalability

The design should allow additional events, categories, users, enrolments and results to be added without changing the fundamental database structure.