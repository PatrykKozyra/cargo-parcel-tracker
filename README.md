# Cargo Parcel Tracker

## Project Purpose

This is an ASP.NET Core MVC web application designed for tracking cargo parcels on crude oil tankers at Shell. The application provides a comprehensive system for managing and monitoring crude oil shipments across different vessels and ports.

## Features

- **Tanker Management**: Track crude oil tanker vessels with details including name, IMO number, capacity, and current status
- **Port Management**: Maintain port information including location, country, and port codes
- **Cargo Parcel Tracking**: Monitor individual cargo parcels with:
  - Parcel identification and reference numbers
  - Associated tanker and origin/destination ports
  - Cargo quantity and grade
  - Loading and discharge dates
  - Current status tracking
  - Special handling notes

## Technology Stack

- **.NET 8**: Latest LTS version of ASP.NET Core
- **Entity Framework Core**: ORM for database operations
- **SQLite**: Development database (easily switchable to SQL Server for production)
- **ASP.NET Core MVC**: Model-View-Controller architecture
- **Bootstrap**: Responsive UI framework

## Project Structure

```
cargo-parcel-tracker/
├── Controllers/        # MVC Controllers
├── Models/            # Domain models and view models
├── Views/             # Razor views
├── Data/              # Database context and migrations
├── wwwroot/           # Static files (CSS, JS, images)
├── Properties/        # Launch settings
├── appsettings.json   # Application configuration
└── Program.cs         # Application entry point
```

## Database Models

### Tanker
- Vessel identification (IMO number, name)
- Capacity and operational status
- Owner information

### Port
- Port details (name, code, country)
- Geographic location

### CargoParcel
- Parcel tracking information
- Tanker assignment
- Origin and destination ports
- Cargo specifications (quantity, grade)
- Timeline (loading/discharge dates)
- Status and notes

## Getting Started

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022, VS Code, or Rider

### Running the Application

1. Navigate to the project directory:
   ```bash
   cd cargo-parcel-tracker
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

5. Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## Configuration

The application uses `appsettings.json` for configuration:
- **Database Connection**: SQLite for development (can be changed to SQL Server)
- **Logging**: Configured for different log levels
- **HTTPS**: Enabled by default for secure communication

## Development

### Database Migrations

To create a new migration after model changes:
```bash
dotnet ef migrations add MigrationName
```

To update the database:
```bash
dotnet ef database update
```

### Switching to SQL Server

Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CargoParcelTracker;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

## License

Internal Shell project - Not for public distribution
