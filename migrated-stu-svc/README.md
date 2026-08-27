# CT — Single Project Application

The entire application uses exactly one project:

    ct.csproj

There are no service-specific `.csproj` files.

## Run locally

    cd ct
    dotnet restore
    dotnet build
    dotnet run

Or from anywhere:

    dotnet run --project /path/to/ct/ct.csproj

## Docker

    docker compose up --build

API:

    http://localhost:5000

The application remains a modular monolith. Services such as Student, Fee,
Attendance, Assessment and Reports can be organized into folders without
creating additional project files.
