# RaceDay API Endpoint Reference

## 1. Overview

This document provides a reference for the planned REST API for the RaceDay Event Management System.

Part 1 focuses on system planning and does not require implementation of the API.

The planned API supports:

* Authentication
* User profiles
* Event management
* Event categories
* Participant enrolments
* Race results

The system uses role-based access control with two main roles:

* Organiser
* Participant

## 2. Authentication

| Method | Route                | Description                                               | Role Required | Request Body                      | Expected Response                          |
| ------ | -------------------- | --------------------------------------------------------- | ------------- | --------------------------------- | ------------------------------------------ |
| POST   | `/api/auth/register` | Registers a new RaceDay user.                             | None          | `fullName, email, password, role` | 201 Created; 400 Bad Request; 409 Conflict |
| POST   | `/api/auth/login`    | Authenticates a user and returns an authentication token. | None          | `email, password`                 | 200 OK; 401 Unauthorized                   |

## 3. User Profile

| Method | Route           | Description                                           | Role Required          | Request Body              | Expected Response                         |
| ------ | --------------- | ----------------------------------------------------- | ---------------------- | ------------------------- | ----------------------------------------- |
| GET    | `/api/users/me` | Retrieves the authenticated user's profile.           | Any authenticated user | None                      | 200 OK; 401 Unauthorized                  |
| PUT    | `/api/users/me` | Updates the authenticated user's profile information. | Any authenticated user | `fullName, contactNumber` | 200 OK; 400 Bad Request; 401 Unauthorized |

## 4. Events

| Method | Route              | Description                                | Role Required | Request Body                                                       | Expected Response                                             |
| ------ | ------------------ | ------------------------------------------ | ------------- | ------------------------------------------------------------------ | ------------------------------------------------------------- |
| GET    | `/api/events`      | Retrieves upcoming RaceDay events.         | None          | None                                                               | 200 OK                                                        |
| GET    | `/api/events/{id}` | Retrieves details for a specific event.    | None          | None                                                               | 200 OK; 404 Not Found                                         |
| POST   | `/api/events`      | Creates a new sporting event.              | Organiser     | `eventName, description, eventDate, location, distance, eventType` | 201 Created; 400 Bad Request; 401 Unauthorized; 403 Forbidden |
| PUT    | `/api/events/{id}` | Updates an event managed by the organiser. | Organiser     | `eventName, description, eventDate, location, distance, eventType` | 200 OK; 400 Bad Request; 403 Forbidden; 404 Not Found         |
| DELETE | `/api/events/{id}` | Deletes an event managed by the organiser. | Organiser     | None                                                               | 204 No Content; 403 Forbidden; 404 Not Found                  |

## 5. Categories

| Method | Route                         | Description                                  | Role Required | Request Body             | Expected Response                                          |
| ------ | ----------------------------- | -------------------------------------------- | ------------- | ------------------------ | ---------------------------------------------------------- |
| GET    | `/api/events/{id}/categories` | Retrieves categories belonging to an event.  | None          | None                     | 200 OK; 404 Not Found                                      |
| POST   | `/api/events/{id}/categories` | Creates a category for an organiser's event. | Organiser     | `categoryName, distance` | 201 Created; 400 Bad Request; 403 Forbidden; 404 Not Found |
| PUT    | `/api/categories/{id}`        | Updates an existing event category.          | Organiser     | `categoryName, distance` | 200 OK; 400 Bad Request; 403 Forbidden; 404 Not Found      |
| DELETE | `/api/categories/{id}`        | Deletes an event category.                   | Organiser     | None                     | 204 No Content; 403 Forbidden; 404 Not Found               |

## 6. Event Enrolments

| Method | Route                         | Description                                             | Role Required | Request Body          | Expected Response                                            |
| ------ | ----------------------------- | ------------------------------------------------------- | ------------- | --------------------- | ------------------------------------------------------------ |
| POST   | `/api/enrolments`             | Enrols a participant into an event category.            | Participant   | `eventId, categoryId` | 201 Created; 400 Bad Request; 401 Unauthorized; 409 Conflict |
| GET    | `/api/enrolments/me`          | Retrieves the authenticated participant's enrolments.   | Participant   | None                  | 200 OK; 401 Unauthorized                                     |
| GET    | `/api/events/{id}/enrolments` | Allows an organiser to view enrolments for their event. | Organiser     | None                  | 200 OK; 403 Forbidden; 404 Not Found                         |
| DELETE | `/api/enrolments/{id}`        | Cancels the participant's own enrolment.                | Participant   | None                  | 204 No Content; 403 Forbidden; 404 Not Found                 |

## 7. Results

| Method | Route                      | Description                                                  | Role Required | Request Body                              | Expected Response                                          |
| ------ | -------------------------- | ------------------------------------------------------------ | ------------- | ----------------------------------------- | ---------------------------------------------------------- |
| POST   | `/api/results`             | Captures a participant's race result.                        | Organiser     | `enrolmentId, finishTime, finishPosition` | 201 Created; 400 Bad Request; 403 Forbidden; 404 Not Found |
| PUT    | `/api/results/{id}`        | Updates or corrects an existing race result.                 | Organiser     | `finishTime, finishPosition`              | 200 OK; 400 Bad Request; 403 Forbidden; 404 Not Found      |
| GET    | `/api/results/me`          | Retrieves the authenticated participant's result history.    | Participant   | None                                      | 200 OK; 401 Unauthorized                                   |
| GET    | `/api/events/{id}/results` | Allows an organiser to view results for their managed event. | Organiser     | None                                      | 200 OK; 403 Forbidden; 404 Not Found                       |

## 8. Role-Based Access Summary

### Organiser

Organisers can:

* Create events.
* Update events they manage.
* Delete events they manage.
* Create event categories.
* Update categories.
* Delete categories.
* View participant enrolments for their events.
* Capture race results.
* Correct race results.
* View results for events they manage.

### Participant

Participants can:

* View available events.
* View event details.
* View event categories.
* Enrol in an event category.
* View their own enrolments.
* Cancel their own enrolments.
* View their own race results.
* Update their own profile.

### Public/Unauthenticated Access

The planned public endpoints allow users to:

* Register.
* Log in.
* View available events.
* View individual event details.
* View event categories.

## 9. HTTP Response Codes

The API uses standard HTTP status codes to communicate the result of each request.

| Status Code      | Meaning                                                                |
| ---------------- | ---------------------------------------------------------------------- |
| 200 OK           | Request completed successfully.                                        |
| 201 Created      | A new resource was successfully created.                               |
| 204 No Content   | Request completed successfully with no response body.                  |
| 400 Bad Request  | The request contains invalid data.                                     |
| 401 Unauthorized | Authentication is required or credentials are invalid.                 |
| 403 Forbidden    | The authenticated user does not have permission to perform the action. |
| 404 Not Found    | The requested resource could not be found.                             |
| 409 Conflict     | The request conflicts with an existing record or rule.                 |

## 10. Security and Access Control

The planned API uses authentication and role-based authorisation.

Authenticated requests will use the user's identity to determine whether the requested operation is permitted.

Organisers are restricted to managing events and results that they are authorised to manage.

Participants are restricted to their own enrolments and result history.

This access-control design helps protect participant information and prevents users from modifying resources belonging to other users.
