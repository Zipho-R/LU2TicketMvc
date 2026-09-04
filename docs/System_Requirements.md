# RaceDay System Requirements

## Overview

The RaceDay Event Management System is designed to manage sporting events, participants, event categories, enrolments and race results.

Part 1 focuses on planning and database design. The actual API and MVC implementation will be completed in a later development phase.

## Functional Requirements

### Authentication

The system must allow users to:

- Register as an Organiser or Participant.
- Log in using their email address and password.
- Receive appropriate authentication credentials after successful login.

### User Profiles

Authenticated users must be able to:

- View their own profile.
- Update their name and contact number.

### Event Management

The system must allow users to:

- View available upcoming events.
- View the details of an individual event.

Organisers must be able to:

- Create events.
- Update events they manage.
- Delete events they manage.

### Category Management

The system must allow users to view the categories available for an event.

Organisers must be able to:

- Create categories for their events.
- Update categories they manage.
- Delete categories they manage.

### Participant Enrolment

Participants must be able to:

- Enrol in an event.
- Select an available category when enrolling.
- View their own enrolments.
- Cancel their own enrolments.

The system must ensure that the selected category belongs to the event being entered.

### Organiser Enrolment Management

Organisers must be able to:

- View enrolments for events they manage.
- Identify the participants enrolled in their events.
- Identify the category selected by each participant.

### Race Results

Organisers must be able to:

- Capture a participant's finish time.
- Capture a participant's finishing position.
- Update or correct a recorded result.
- View results for events they manage.

Participants must be able to:

- View their own race results.

## Role-Based Access Requirements

### Organiser

An Organiser can:

- Manage their own events.
- Manage categories for their events.
- View enrolments for their events.
- Capture and update results.
- View results for their events.

### Participant

A Participant can:

- View available events.
- Manage their own profile.
- Enrol in events.
- Select event categories.
- View their own enrolments.
- Cancel their own enrolments.
- View their own results.

## Data Integrity Requirements

The database must:

- Use primary keys for all entities.
- Use foreign keys to maintain relationships.
- Prevent duplicate user email addresses.
- Ensure categories belong to valid events.
- Ensure enrolments reference valid participants, events and categories.
- Ensure the category selected during enrolment belongs to the selected event.
- Prevent an enrolment from having more than one result.
- Restrict role and status values to valid options.

## Non-Functional Requirements

The system should:

- Protect authenticated resources through role-based authorisation.
- Maintain data consistency through database constraints.
- Provide clear validation for invalid user input.
- Use meaningful HTTP status codes in API responses.
- Be maintainable through a clear project and documentation structure.
- Be version controlled using Git and GitHub.
- Use automated GitHub Actions checks to validate the repository.

## Part 1 Scope

The following are included in Part 1:

- System planning.
- ERD design.
- Database design.
- SQL database script.
- API endpoint planning.
- Repository setup.
- GitHub Actions validation.
- Project documentation.

API implementation, controllers, MVC functionality and other application development will be completed in a later phase.