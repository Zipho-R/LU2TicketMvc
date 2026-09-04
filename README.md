# RaceDay Event Management System

## Part 1 – System Planning and Database

RaceDay is an event management system designed to support the management of sporting events, participant enrolments, event categories and race results.

Part 1 focuses on system planning, database design and API endpoint planning. The API and MVC implementation will be completed in a later phase.

### User Roles

The system supports two main user roles:

- **Organiser** – creates and manages events and categories, views participant enrolments, and captures race results.
- **Participant** – views events, enrols in event categories, manages their profile and views their own results.

### Part 1 Deliverables

The following Part 1 documents are stored in the `docs` folder:

- `RaceDay_ERD.pdf` – Entity Relationship Diagram
- `RaceDay_API_Endpoint_Plan.pdf` – API endpoint planning
- `RaceDay_Database.sql` – SQL Server database creation and seed script

### Supporting Documentation

Additional documentation for the Part 1 planning and database work is available below:

- [Database Setup Guide](docs/Database_Setup.md)
- [ERD Description](docs/ERD_Description.md)
- [API Endpoint Reference](docs/API_Endpoint_Reference.md)

### Repository Structure

```text
LU2TicketMvc/
├── .github/
│   └── workflows/
│       ├── dotnet.yml
│       └── part1-ci.yml
├── docs/
│   ├── RaceDay_ERD.pdf
│   ├── RaceDay_API_Endpoint_Plan.pdf
│   └── RaceDay_Database.sql
├── Models/
├── Controllers/
├── Views/
├── wwwroot/
└── README.md

# LU2TicketMvc
This project is a simple ASP.NET Core MVC application for submitting a student support ticket.

The project is divided into three main parts:

Model: stores the ticket information

Controller: receives and processes the information

View: displays the form and confirmation page

## Project folders

Models

Open:

Models
└── Ticket.cs

Ticket.cs contains the information used by the ticket form, such as:

Ticket ID

Student name

Student email

Support issue

Ticket status

Add new ticket information here first.

For example, to add a module code:

public string ModuleCode { get; set; } = "";

After adding a new property, also add it to the form and confirmation View.

Controllers

Open:

Controllers
└── HomeController.cs

The Controller decides what happens when a page opens or when a form is submitted.

The Index action opens the ticket form:

[HttpGet]
public IActionResult Index()

The SubmitTicket action receives the completed form:

[HttpPost]
public IActionResult SubmitTicket(Ticket ticket)

Use the Controller when you need to:

open a View

receive form information

check submitted information

assign a ticket number or status

send information to another View

Views

Open:

Views
└── Home
    ├── Index.cshtml
    └── TicketConfirmation.cshtml

Index.cshtml contains the student support ticket form.

Use this file when you need to:

add another input field

change labels

add validation messages

change the form layout

TicketConfirmation.cshtml displays the submitted ticket.

Use this file when you need to show any new information added to the Model.

Shared Views

Open:

Views
└── Shared
    └── _Layout.cshtml

_Layout.cshtml controls the layout used by the whole website, including the navigation bar and footer.

wwwroot

Open:

wwwroot
├── css
└── js

Use wwwroot/css/site.css for your own styling.

How the application works

The student opens the form
        ↓
HomeController runs the Index action
        ↓
Index.cshtml displays the form
        ↓
The student submits the form
        ↓
HomeController runs SubmitTicket
        ↓
TicketConfirmation.cshtml displays the result

Continue with the project

Complete the following changes:

Add a ModuleCode property to Ticket.cs.

Add a Module Code field to Index.cshtml.

Display the Module Code in TicketConfirmation.cshtml.

Add validation to the student name, email and support issue.

Run the project and test the form.

Commit and push your changes to GitHub.

Important

When adding a new property, update all the places that use it:

Model
↓
Form View
↓
Confirmation View

Make one change at a time, run the application and fix any errors before moving to the next change.

GitHub

After completing and testing a change:

Open Git Changes in Visual Studio.

Enter a clear commit message.

Select Commit All.

Select Push.

Examples:

Added module code to ticket
Added ticket validation
Updated confirmation page

Do not upload only screenshots. The full Visual Studio solution must be pushed to GitHub.
