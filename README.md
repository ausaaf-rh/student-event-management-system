# University Gatherings API

A comprehensive ASP.NET Core Web API for managing university events, student registrations, and feedback collection. This system provides endpoints for creating, managing, and tracking academic gatherings with full CRUD operations.

## 🚀 Features

- **Event Management**: Create, update, delete, and search university gatherings
- **Student Profiles**: Manage learner profiles and academic information
- **Registration System**: Handle event enrollments and capacity management
- **Feedback Collection**: Collect and process gathering feedback with ratings
- **Advanced Search**: Filter events by category, date range, and availability
- **RESTful API**: Clean, well-documented API endpoints
- **Swagger Documentation**: Interactive API documentation and testing
- **Entity Framework Core**: Database management with code-first approach

## 🏗️ Architecture

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic layer with dependency injection
- **Models**: Entity framework models with data annotations
- **Data Layer**: Entity Framework DbContext for database operations

## 📋 Prerequisites

Before running this application, ensure you have the following installed:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [Git](https://git-scm.com/) (optional, for version control)

## ⚙️ Installation & Setup

### 1. Clone the Repository

```bash
git clone <your-repository-url>
cd StudentEventAPI
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Database Connection

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=UniversityGatheringsDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**For other database providers:**

- **SQL Server**: `"Server=localhost;Database=UniversityGatheringsDb;Trusted_Connection=true;"`
- **MySQL**: Install `Pomelo.EntityFrameworkCore.MySql` and update connection string
- **PostgreSQL**: Install `Npgsql.EntityFrameworkCore.PostgreSQL` and update connection string

### 4. Install Entity Framework Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

### 5. Create and Apply Database Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update
```

### 6. Build the Project

```bash
dotnet build
```

### 7. Run the Application

```bash
dotnet run
```

The application will start and display:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5000
```

## 🌐 Accessing the Application

### Swagger UI (API Documentation)
- **HTTPS**: https://localhost:7001/swagger
- **HTTP**: http://localhost:5000/swagger

### API Base URLs
- **HTTPS**: https://localhost:7001/api
- **HTTP**: http://localhost:5000/api

## 📚 API Endpoints

### University Gatherings (`/api/university-gatherings`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Fetch all gatherings |
| GET | `/{id}` | Get specific gathering |
| POST | `/` | Create new gathering |
| PUT | `/{id}` | Update existing gathering |
| DELETE | `/{id}` | Remove gathering |
| GET | `/search?query={term}` | Search gatherings |
| GET | `/classifications/{category}` | Get by category |
| GET | `/timeframe?start={date}&end={date}` | Get by date range |
| GET | `/{id}/capacity-status` | Check availability |

### Learner Profiles (`/api/learner-profiles`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all student profiles |
| GET | `/{id}` | Get specific student |
| POST | `/` | Create new student profile |
| PUT | `/{id}` | Update student information |

### Gathering Enrollments (`/api/gathering-enrollments`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all registrations |
| POST | `/` | Register student for event |
| DELETE | `/{id}` | Cancel registration |
| GET | `/student/{studentId}` | Get student's registrations |
| GET | `/gathering/{eventId}` | Get event registrations |

### Gathering Feedback (`/api/gathering-feedback`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all feedback |
| POST | `/` | Submit feedback |
| GET | `/gathering/{eventId}` | Get event feedback |
| GET | `/gathering/{eventId}/summary` | Get feedback summary |

## 🧪 Testing with Sample Data

### Create a New Gathering (POST)

```json
{
  "eventTitle": "Annual Tech Conference 2025",
  "eventDescription": "Explore the latest in technology and innovation",
  "eventLocation": "Main Auditorium, Building A",
  "scheduledDateTime": "2025-07-15T09:00:00",
  "maxCapacity": 300,
  "category": 1,
  "organizerEmail": "tech.organizer@university.edu"
}
```

### Create a Student Profile (POST)

```json
{
  "fullName": "Jane Smith",
  "email": "jane.smith@student.university.edu",
  "academicFaculty": "Computer Science",
  "yearLevel": "Senior"
}
```

### Register for Event (POST)

```json
{
  "studentId": 1,
  "eventId": 1,
  "registrationNotes": "Looking forward to the networking sessions"
}
```

### Submit Feedback (POST)

```json
{
  "eventId": 1,
  "studentId": 1,
  "presentationRating": 5,
  "organizationRating": 4,
  "venueRating": 5,
  "overallSatisfaction": 5,
  "detailedComments": "Excellent event with great speakers and networking opportunities!",
  "submissionDate": "2025-07-15T17:00:00"
}
```

## 🗄️ Database Schema

### Tables Created:
- `UniversityGatherings` - Event information
- `Students` - Student/learner profiles  
- `GatheringEnrollments` - Event registrations
- `GatheringFeedbacks` - Event feedback and ratings

## 🔧 Configuration Options

### Environment Variables

```bash
# Development environment
export ASPNETCORE_ENVIRONMENT=Development

# Production connection string
export ConnectionStrings__DefaultConnection="Your-Production-Connection-String"
```

### appsettings.json Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Your-Database-Connection-String"
  },
  "AllowedHosts": "*"
}
```

## 🚀 Deployment

### Development Environment
```bash
dotnet run --environment Development
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
```

### Docker Deployment (Optional)

Create a `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["StudentEventAPI.csproj", "."]
RUN dotnet restore "StudentEventAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "StudentEventAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentEventAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentEventAPI.dll"]
```

## 🛠️ Troubleshooting

### Common Issues

1. **Port Already in Use**
   ```bash
   dotnet run --urls "https://localhost:7002;http://localhost:5002"
   ```

2. **Database Connection Issues**
   - Verify SQL Server is running
   - Check connection string in appsettings.json
   - Ensure database exists or run migrations

3. **Migration Issues**
   ```bash
   dotnet ef database drop --force
   dotnet ef migrations remove
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Build Errors**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

### Logs Location
- Development: Console output
- Production: Configure logging providers in `Program.cs`

## 📝 Development Notes

### Project Structure
```
StudentEventAPI/
├── Controllers/          # API Controllers
├── Models/              # Entity Models
├── Services/            # Business Logic
├── Data/               # Database Context
├── Migrations/         # EF Migrations
├── Program.cs          # Application Entry Point
├── appsettings.json    # Configuration
└── StudentEventAPI.csproj # Project File
```

### Adding New Features
1. Create model in `Models/` folder
2. Add DbSet to `AppDbContext`
3. Create service interface and implementation
4. Register service in `Program.cs`
5. Create controller with endpoints
6. Add migration and update database
