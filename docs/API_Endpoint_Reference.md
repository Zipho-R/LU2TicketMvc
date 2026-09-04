# RaceDay API Endpoint Reference

## Overview

This document provides a reference for the API endpoints planned for the RaceDay Event Management System.

The API will support authentication, user profiles, event management, event categories, participant enrolments and race results.

The detailed API planning document is available at:

`docs/RaceDay_API_Endpoint_Plan.pdf`

## Authentication

| Method | Route | Role | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | None | Register a new organiser or participant |
| POST | `/api/auth/login` | None | Authenticate a user and return an access token |

## User Profile

| Method | Route | Role | Purpose |
|---|---|---|---|
| GET | `/api/users/me` | Any authenticated user | View the current user's profile |
| PUT | `/api/users/me` | Any authenticated user | Update the current user's profile |

## Events

| Method | Route | Role | Purpose |
|---|---|---|---|
| GET | `/api/events` | None | View all upcoming events |
| GET | `/api/events/{id}` | None | View details for a specific event |
| POST | `/api/events` | Organiser | Create a new event |
| PUT | `/api/events/{id}` | Organiser | Update an event managed by the organiser |
| DELETE | `/api/events/{id}` | Organiser | Delete an event managed by the organiser |

## Event Categories

| Method | Route | Role | Purpose |
|---|---|---|---|
| GET | `/api/events/{id}/categories` | None | View categories belonging to an event |
| POST | `/api/events/{id}/categories` | Organiser | Create a category for an organiser's event |
| PUT | `/api/categories/{id}` | Organiser | Update an event category |
| DELETE | `/api/categories/{id}` | Organiser | Delete an event category |

## Event Enrolments

| Method | Route | Role | Purpose |
|---|---|---|---|
| POST | `/api/enrolments` | Participant | Enrol in an event and select a category |
| GET | `/api/enrolments/me` | Participant | View the participant's own enrolments |
| GET | `/api/events/{id}/enrolments` | Organiser | View enrolments for an organiser's event |
| DELETE | `/api/enrolments/{id}` | Participant | Cancel the participant's own enrolment |

## Results

| Method | Route | Role | Purpose |
|---|---|---|---|
| POST | `/api/results` | Organiser | Capture a participant's race result |
| PUT | `/api/results/{id}` | Organiser | Update or correct a race result |
| GET | `/api/results/me` | Participant | View the participant's own results |
| GET | `/api/events/{id}/results` | Organiser | View results for an organiser's event |

## Role-Based Access

The planned API uses role-based access control.

### Organiser

Organisers can:

- Create and manage their events.
- Create and manage event categories.
- View enrolments for their events.
- Capture and update participant results.
- View results for their managed events.

### Participant

Participants can:

- View available events.
- Manage their own profile.
- Enrol in event categories.
- View their own enrolments.
- Cancel their own enrolments.
- View their own race results.

### Public Access

Unauthenticated users can:

- View available events.
- View individual event details.
- View event categories.

Authentication and authorisation will be implemented during the API development phase.