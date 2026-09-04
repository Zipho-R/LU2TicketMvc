# RaceDay Database Setup

## Overview

The RaceDay database supports the management of sporting events, event categories, participant enrolments and race results.

The database is designed for Microsoft SQL Server and follows the entity structure defined in the RaceDay Entity Relationship Diagram.

## Database Entities

The database contains the following six entities:

1. Role
2. User
3. Event
4. Category
5. Enrolment
6. Result

## Database Script

The complete SQL Server database script is located at:

`docs/RaceDay_Database.sql`

The script creates the required tables, primary keys, foreign keys, constraints and sample data.

## Requirements

To run the database script, use:

- Microsoft SQL Server
- SQL Server Management Studio (SSMS), or another SQL Server-compatible client

## Running the Script

1. Open SQL Server Management Studio.
2. Connect to your SQL Server instance.
3. Open `docs/RaceDay_Database.sql`.
4. Select the appropriate database or create a new database for testing.
5. Run the complete script.
6. Check that all six tables are created successfully.
7. Review the verification queries at the end of the script.

The script should be tested on a clean database to ensure that the complete database structure can be created successfully.

## Sample Data

The script includes realistic sample data for testing, including:

- 2 Organisers
- 2 Participants
- 3 Events
- Event categories
- Participant enrolments
- Sample race results

## Database Constraints

The database uses:

- Primary keys to uniquely identify records.
- Foreign keys to maintain relationships between entities.
- `NOT NULL` constraints for required information.
- `UNIQUE` constraints where duplicate values should not be allowed.
- `DEFAULT` values for fields such as registration and enrolment dates.
- `CHECK` constraints to restrict certain values to valid options.

## Relationship Integrity

The database ensures that:

- Users are assigned valid roles.
- Events are managed by organisers.
- Categories belong to events.
- Participants enrol in events and select categories.
- Categories selected during enrolment belong to the relevant event.
- Each enrolment can have at most one result.
- Results are captured by an organiser.