# ShipIt Inventory Management

## Setup Instructions
Open the project in VSCode.
VSCode should automatically set up and install everything you'll need apart from the database connection!

### Setting up the Database.
Create 2 new postgres databases from your terminal:
- sudo -u postgres psql
- CREATE DATABASE shipit_dev
- CREATE DATABASE shipit_test
- \q   (to exit)

Ask a team member for a dump of the production databases to create and populate your tables:
- psql -U postgres -d postgresql://localhost:5432/shipit_dev -f ShipIt-database-dump.sql
- psql -U postgres -d postgresql://localhost:5432/shipit_test -f ShipIt-database-dump.sql

Then for each of the projects, add a `.env` file at the root of the project with the following:
```
POSTGRES_CONNECTION_STRING=Server=127.0.0.1;Port=5432;Database=[database_name];User Id=[your_database_user]; Password=[your_database_password];
```

## Running The API
Once set up, simply run dotnet run in the ShipIt directory.

## Running The Tests
To run the tests you should be able to run dotnet test in the ShipItTests directory.

## Deploying to Production
TODO
