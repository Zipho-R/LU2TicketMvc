# RaceDay

## Project Description
RaceDay is a web-based event management platform for South African road running, walking and cycling events.
This repository contains Part 1 (planning) and Part 2 (the RaceDay RESTful API) of the PROG6212 Portfolio of Evidence.

## User Roles

### Organiser
- Creates, updates and deletes their own Events.
- Manages Categories for the Events they own.
- Views enrolments for their own Events.
- Captures finish times and finishing positions (Results) for their own Events.

### Participant
- Browses Events and Categories.
- Enrols in an Event by selecting a Category.
- Views their own enrolments and their own Results.

Both roles can view and update their own profile.

## Part 1
See the `docs/` folder for the ERD, API endpoint plan and SQL database script submitted for Part 1.

## Part 2

### Technologies Used
- ASP.NET Core 8 Web API
- Entity Framework Core 8 (SQL Server, Code First)
- ASP.NET Core Session (server-side authentication state)
- Swashbuckle / Swagger
- xUnit, Microsoft.AspNetCore.Mvc.Testing, EF Core InMemory (unit + integration tests)
- GitHub Actions (CI)

### Project Structure
```
RaceDay
|-- docs                      Part 1 planning documents
|-- RaceDay.Api                The Web API
|   |-- Controllers
|   |-- Data                   RaceDayDbContext
|   |-- Filters                RequireRoleAttribute (session-based auth/role checks)
|   |-- Models                 EF Core entities
|   `-- Program.cs
|-- RaceDay.Contracts          Shared DTOs (used by RaceDay.Api and, later, the MVC app)
|-- RaceDay.Api.Tests          xUnit tests
`-- .github/workflows/ci.yml   CI pipeline
```

### Database Setup

1. Install SQL Server Express and SQL Server Management Studio (SSMS).
2. Confirm that the connection string in `RaceDay.Api/appsettings.json` matches your SQL Server instance.
3. From the solution root folder, restore the project packages:

   ```powershell
   dotnet restore
   ```

4. Apply the existing Entity Framework Core migration to create the database:

   ```powershell
   dotnet ef database update --project .\RaceDay.Api --startup-project .\RaceDay.Api
   ```

5. Open SQL Server Management Studio, connect to your SQL Server instance, and confirm that `RaceDayDb` and its tables exist.

The initial migration is named `InitialCreate`. If you need to create a new migration after changing the entity models, run:

```powershell
dotnet ef migrations add YourMigrationName --project .\RaceDay.Api --startup-project .\RaceDay.Api
```


### How to Run the API
```
dotnet restore
dotnet build
dotnet run --project RaceDay.Api
```
Then open `/swagger` in the browser (the launch profile opens it automatically in Visual Studio).

### Authentication
- `POST /api/auth/register` - registers an Organiser or Participant. Passwords are hashed with
  ASP.NET Core's `PasswordHasher` and the plain-text password is never stored.
- `POST /api/auth/login` - verifies the password hash and stores `UserId` and `Role` in a
  server-side session.
- `POST /api/auth/logout` - clears the session.

### Session Management
On successful login, `UserId` and `Role` are written to the ASP.NET Core session. The `RequireRoleAttribute`
filter (in `RaceDay.Api/Filters`) reads these values on every protected request:
- No `UserId` in session -> `401 Unauthorized`.
- `UserId` present but role not permitted for the endpoint -> `403 Forbidden`.

### API Endpoints

| Area | Endpoint | Access |
|---|---|---|
| Auth | `POST /api/auth/register` | Public |
| Auth | `POST /api/auth/login` | Public |
| Auth | `POST /api/auth/logout` | Authenticated |
| Profile | `GET /api/profile/me` | Authenticated (own account only) |
| Profile | `PUT /api/profile/me` | Authenticated (own account only) |
| Events | `GET /api/events` | Public / both roles |
| Events | `GET /api/events/{id}` | Public / both roles |
| Events | `GET /api/events/mine` | Organiser |
| Events | `POST /api/events` | Organiser |
| Events | `PUT /api/events/{id}` | Organiser (owner only) |
| Events | `DELETE /api/events/{id}` | Organiser (owner only) |
| Categories | `GET /api/events/{eventId}/categories` | Both roles |
| Categories | `POST /api/events/{eventId}/categories` | Organiser (owner only) |
| Categories | `PUT /api/categories/{id}` | Organiser (owner only) |
| Categories | `DELETE /api/categories/{id}` | Organiser (owner only) |
| Enrolments | `POST /api/enrolments` | Participant |
| Enrolments | `GET /api/enrolments/mine` | Participant |
| Enrolments | `GET /api/enrolments/event/{eventId}` | Organiser (owner only) |
| Enrolments | `PUT /api/enrolments/{id}/status` | Organiser (owner only) |
| Results | `POST /api/results` | Organiser (owner only) |
| Results | `GET /api/results/mine` | Participant |
| Results | `GET /api/results/event/{eventId}` | Organiser (owner only) |

### Swagger
Swagger UI is available at `/swagger` in Development and lists Auth, Profile, Events, Categories, Enrolments and
Results. Use it to walk through: register -> login -> create Event -> create Categories -> enrol as Participant ->
view enrolments -> capture a Result -> view personal Results.

### Unit Testing
`RaceDay.Api.Tests` uses `WebApplicationFactory<Program>` with an isolated EF Core InMemory database per test class, so
tests exercise the real controllers, filters, session pipeline and password hashing without touching SQL Server.
There are 64 tests covering both success and failure paths:
- **Authentication:** valid/duplicate/invalid-role/missing-field registration, password stored hashed, valid and invalid login, logout clears the session.
- **Profile:** both roles view and update their own profile; duplicate email (`409`), missing fields (`400`), not logged in (`401`).
- **Events:** both roles can view; only Organisers create (`201`/`403`); owner-only update and delete; `/mine` returns only own events; validation (`400`); cannot delete an Event that has enrolments (`409`).
- **Categories:** both roles view; owner-only create/update/delete (`403` for other Organisers and Participants); invalid distance (`400`); duplicate name (`409`); missing Event (`404`).
- **Enrolments:** Participant enrols (`201`); Organisers cannot enrol (`403`); category must belong to the Event (`400`); one enrolment per Event (`409`); Participants only see their own; Organisers only manage their own Events' enrolments and statuses.
- **Results:** Organiser captures results for their own Events only; Participants cannot (`403`); duplicates and invalid positions rejected (`400`); Participants only see their own results.

Run locally with:
```
dotnet test RaceDay.sln
```

### HTTP Status Codes Used
| Code | Used when |
|---|---|
| `200` / `201` / `204` | Success / resource created / deleted |
| `400` | Invalid or missing data, invalid role, invalid category for the Event |
| `401` | Not logged in |
| `403` | Wrong role, or an Organiser acting on an Event they do not own |
| `404` | Resource does not exist |
| `409` | Duplicate (email, category name, enrolment) or deleting something that still has enrolments |

### Changes From Part 1
- Categories carry a `DistanceKm` value (for example 5 km or 10 km) so a category can represent a distance as well as an age group.
- A Participant may enrol only **once per Event** (unique on `ParticipantId` + `EventId`), matching the database design. A second attempt returns `409`.
- Events and Categories that already have enrolments cannot be deleted (`409`), which protects the enrolment history.
- New enrolments are created with the status `Confirmed`; the Organiser can change it to `Pending` or `Cancelled`.

### CI/CD
`.github/workflows/ci.yml` runs on every push and pull request to `main`: restore -> build -> test.

### GitHub Actions Screenshot
_Add a screenshot of the green CI run here once pushed._

### Video Presentation
_Add the unlisted YouTube link here._

## Part 1 Supporting Documents

The original Part 1 planning documents and supporting material are preserved in `docs/`:

- [RaceDay Entity Relationship Diagram](docs/RaceDay_ERD.pdf)
- [API Endpoint Plan](docs/RaceDay_API_Endpoint_Plan.pdf)
- [SQL Server Database Script](docs/RaceDay_Database.sql)
- [Database Setup Guide](docs/Database_Setup.md)
- [ERD Description](docs/ERD_Description.md)
- [API Endpoint Reference](docs/API_Endpoint_Reference.md)
- [Data Dictionary](docs/Data_Dictionary.md)
- [RaceDay Requirements](docs/RaceDay_Requirements.md)

## Part 1 CI Evidence

The repository retains the screenshots from the Part 1 GitHub Actions runs:

- [Screenshot 3](Screenshots/Screenshot%20%283%29.png)
- [Screenshot 4](Screenshots/Screenshot%20%284%29.png)
- [Screenshot 5](Screenshots/Screenshot%20%285%29.png)
- [Screenshot 6](Screenshots/Screenshot%20%286%29.png)

These are existing Part 1 evidence. Add a separate screenshot of the successful Part 2 CI run when it is available.

## Part 1 Video Demonstration

Existing Part 1 video: https://youtu.be/ku8yIwXTC8M

Add the Part 2 API demonstration link under the Video Presentation section above when it is ready.
## Part 1 Supporting Documents

The original Part 1 planning documents and supporting material are preserved in `docs/`:

- [RaceDay Entity Relationship Diagram](docs/RaceDay_ERD.pdf)
- [API Endpoint Plan](docs/RaceDay_API_Endpoint_Plan.pdf)
- [SQL Server Database Script](docs/RaceDay_Database.sql)
- [Database Setup Guide](docs/Database_Setup.md)
- [ERD Description](docs/ERD_Description.md)
- [API Endpoint Reference](docs/API_Endpoint_Reference.md)
- [Data Dictionary](docs/Data_Dictionary.md)
- [RaceDay Requirements](docs/RaceDay_Requirements.md)

## Part 1 CI Evidence

The repository retains the screenshots from the Part 1 GitHub Actions runs:

- [Screenshot 3](Screenshots/Screenshot%20%283%29.png)
- [Screenshot 4](Screenshots/Screenshot%20%284%29.png)
- [Screenshot 5](Screenshots/Screenshot%20%285%29.png)
- [Screenshot 6](Screenshots/Screenshot%20%286%29.png)

## Part 2 CI Evidence
- [Screenshot 16 — Part 2](Screenshots/Screenshot%20%2816%29.png)


## Part 1 Video Demonstration

Existing Part 1 video: https://youtu.be/ku8yIwXTC8M

## Part 1 Video Demonstration

Part 2 video: https://youtu.be/c6g5zSujRGI

