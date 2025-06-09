# Event Registration System

A comprehensive event management system built with ASP.NET Core that allows users to create events and manage participant registrations for both individual and company participants.

🚀 **Live Demo**: [View Application](https://web-production-490ea.up.railway.app)

## Features

- 📅 **Event Management**: Create, update, delete, and view events
- 👥 **Participant Registration**: Support for both individual and company participants
- 💳 **Payment Methods**: Multiple payment options for registrations
- 🔍 **Event Filtering**: View upcoming and past events separately
- ✅ **Comprehensive Testing**: 58+ unit tests covering all functionality
- 🏗️ **Clean Architecture**: Domain-driven design with proper separation of concerns

## Project Structure

```
EventRegistration/
├── EventRegistration.Application/     # Application services and DTOs
├── EventRegistration.Domain/          # Domain models and interfaces
├── EventRegistration.Infrastructure/  # Data access and repositories
├── EventRegistration.Tests/          # Unit and integration tests
└── WebApplication1/                   # Web UI (ASP.NET Core MVC)
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express/LocalDB for local development)
- [PostgreSQL](https://www.postgresql.org/) (for Railway deployment)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd EventRegistration
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Database Setup

The application supports both SQL Server (for local development) and PostgreSQL (for Railway deployment). The database provider is automatically detected based on the connection string.

**For Local Development (SQL Server):**

Update the connection string in `WebApplication1/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EventRegistrationDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**For Railway Deployment (PostgreSQL):**

The application automatically uses PostgreSQL when deployed to Railway. See the [Railway Setup Guide](railway-setup.md) for detailed instructions.

### 4. Apply Database Migrations

```bash
cd WebApplication1
dotnet ef database update
```

### 5. Seed Test Data (Optional)

Load sample data using the provided SQL script:

```bash
# Using SQL Server Management Studio or Azure Data Studio
# Execute the test_data.sql file against your database
```

## Running the Application
 
### Development Mode

```bash
cd WebApplication1
dotnet run
```

The application will start and be available at:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Production Mode

```bash
cd WebApplication1
dotnet run --environment Production
```

### Using Visual Studio Code

1. Open the workspace file: `EventRegistration.code-workspace`
2. Press `F5` to start debugging
3. Or use `Ctrl+Shift+P` → "Tasks: Run Task" → Select a run task

## Running Tests

### Run All Tests

```bash
# From the root directory
dotnet test
```

### Run Tests with Detailed Output

```bash
dotnet test --logger "console;verbosity=normal"
```

### Run Specific Test Classes

```bash
# Run only EventService tests
dotnet test --filter "FullyQualifiedName~EventServiceTests"

# Run only ParticipantService tests
dotnet test --filter "FullyQualifiedName~ParticipantServiceTests"

# Run only Integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run only Performance tests
dotnet test --filter "FullyQualifiedName~PerformanceTests"

# Run only Validation tests
dotnet test --filter "FullyQualifiedName~ValidationAndEdgeCaseTests"
```

### Run Specific Test Methods

```bash
# Run a specific test method
dotnet test --filter "FullyQualifiedName=EventRegistration.Tests.EventServiceTests.GetUpcomingEvents_ShouldReturnOrderedUpcomingEvents"

# Run tests containing specific keywords
dotnet test --filter "Name~CreateEvent"
dotnet test --filter "Name~AddParticipant"
```

### Test Filter Explanation

The `FullyQualifiedName` consists of:

- **Namespace**: `EventRegistration.Tests`
- **Class**: `EventServiceTests`
- **Method**: `GetUpcomingEvents_ShouldReturnOrderedUpcomingEvents`

**Filter Operators:**

- `~` (contains): `FullyQualifiedName~EventService` matches any test with "EventService" in the name
- `=` (equals): `FullyQualifiedName=EventRegistration.Tests.EventServiceTests.GetUpcomingEvents_ShouldReturnOrderedUpcomingEvents` matches exact test
- `Name~keyword`: Matches test method names containing "keyword"

### Practical Examples

```bash
# Run all tests related to event creation
dotnet test --filter "Name~CreateEvent"

# Run all tests that check for exceptions
dotnet test --filter "Name~ShouldThrowException"

# Run all tests for a specific domain model
dotnet test --filter "FullyQualifiedName~DomainModelTests"

# Run all async tests
dotnet test --filter "Name~Async"

# Run tests by category (using traits, if implemented)
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Unit"
```

### Test Coverage

The project includes comprehensive test coverage with **58+ tests** covering:

- ✅ **Unit Tests**: Service layer testing with mocked dependencies
- ✅ **Integration Tests**: End-to-end workflow testing
- ✅ **Domain Model Tests**: Entity behavior and validation
- ✅ **Performance Tests**: Large dataset handling
- ✅ **Concurrency Tests**: Multi-threaded operation testing
- ✅ **Validation Tests**: Edge cases and error handling

### Test Categories

1. **EventServiceTests** (21 tests) - Event CRUD operations and business logic
2. **ParticipantServiceTests** (22 tests) - Participant registration and management
3. **DomainModelTests** (11 tests) - Domain entity behavior
4. **IntegrationTests** (4 tests) - Cross-service workflows
5. **ValidationAndEdgeCaseTests** (10 tests) - Edge cases and error scenarios
6. **PerformanceTests** (2 tests) - Performance validation
7. **ConcurrencyTests** (2 tests) - Concurrent operation handling

## Deployment

### Railway Deployment

This application is configured for easy deployment on [Railway](https://railway.app):

#### Quick Deploy to Railway

1. **Fork this repository** to your GitHub account

2. **Create a Railway account** at [railway.app](https://railway.app)

3. **Deploy from GitHub:**
   - Click "New Project" in Railway
   - Select "Deploy from GitHub repo"
   - Choose your forked repository
   - Railway will automatically detect the .NET application

4. **Add PostgreSQL database:**
   - In your Railway project, click "New Service"
   - Select "Database" → "PostgreSQL"
   - Railway will automatically provide the connection string

5. **Configure environment variables:**
   - Go to your app service → "Variables" tab
   - Railway should automatically set `DATABASE_URL`
   - If needed, manually add: `ConnectionStrings__DefaultConnection` with your PostgreSQL connection string

6. **Deploy!**
   - Railway will automatically build and deploy your application
   - Your app will be available at `https://your-app-name.up.railway.app`

#### Database Configuration

The application automatically detects the database provider:
- **PostgreSQL**: Used when connection string contains "postgresql" (Railway deployment)
- **SQL Server**: Used for local development
- **SQLite**: Fallback option with persistent storage via Railway volumes

For detailed Railway setup instructions, see [Railway Setup Guide](railway-setup.md).

#### Other Deployment Options

For alternative deployment options including Azure App Service, Render, and more, see [DEPLOYMENT.md](DEPLOYMENT.md).

## Code Formatting

The project uses CSharpier for code formatting:

### Format All Files

```bash
dotnet csharpier format .
```

### Check Formatting

```bash
dotnet csharpier check .
```

### Using VS Code Tasks

- `Ctrl+Shift+P` → "Tasks: Run Task" → "Format C# Files"
- `Ctrl+Shift+P` → "Tasks: Run Task" → "Check C# Formatting"

## API Endpoints

### Events

- `GET /` - Homepage with upcoming events
- `GET /Events` - List all events
- `GET /Events/Details/{id}` - Event details with participants
- `GET /Events/Create` - Create event form
- `POST /Events/Create` - Create new event
- `GET /Events/Edit/{id}` - Edit event form
- `POST /Events/Edit/{id}` - Update event
- `POST /Events/Delete/{id}` - Delete event

### Participants

- `GET /Events/{eventId}/Participants` - List event participants
- `GET /Events/{eventId}/Participants/Add` - Add participant form
- `POST /Events/{eventId}/Participants/Add` - Add new participant
- `GET /Events/{eventId}/Participants/{id}/Edit` - Edit participant form
- `POST /Events/{eventId}/Participants/{id}/Edit` - Update participant
- `POST /Events/{eventId}/Participants/{id}/Delete` - Delete participant

## Architecture

### Clean Architecture Layers

1. **Domain Layer** (`EventRegistration.Domain`)

   - Entities: `Event`, `Participant`, `EventParticipant`, `PaymentMethod`
   - Repository interfaces
   - Domain logic and validation

2. **Application Layer** (`EventRegistration.Application`)

   - Services: `EventService`, `ParticipantService`
   - DTOs and ViewModels
   - Business logic orchestration

3. **Infrastructure Layer** (`EventRegistration.Infrastructure`)

   - Entity Framework DbContext with multi-database support
   - Repository implementations
   - Data access logic
   - Automatic database provider detection (SQL Server, PostgreSQL, SQLite)

4. **Presentation Layer** (`WebApplication1`)
   - MVC Controllers and Views
   - Web API endpoints
   - User interface

### Key Design Patterns

- **Repository Pattern**: Data access abstraction
- **Service Pattern**: Business logic encapsulation
- **DTO Pattern**: Data transfer objects for API communication
- **Dependency Injection**: Loose coupling between layers

## Database Schema

### Core Tables

- **Events**: Event information (Name, Time, Location, etc.)
- **Participants**: Individual and company participant data
- **EventParticipants**: Many-to-many relationship with additional info
- **PaymentMethods**: Available payment options

### Relationships

- Events ↔ Participants (Many-to-Many through EventParticipants)
- EventParticipants → PaymentMethods (Many-to-One)

## Development Guidelines

### Adding New Features

1. **Domain First**: Add entities and interfaces in the Domain layer
2. **Application Services**: Implement business logic in Application layer
3. **Infrastructure**: Add repository implementations
4. **Tests**: Write comprehensive unit tests
5. **UI**: Add controllers and views in the Web layer

### Testing Best Practices

- Mock all external dependencies
- Test both success and failure scenarios
- Use parameterized tests for multiple input scenarios
- Include integration tests for critical workflows
- Maintain high test coverage

## Troubleshooting

### Common Issues

1. **Database Connection Issues**

   - **Local Development**: Verify SQL Server is running and check connection string in `appsettings.json`
   - **Railway Deployment**: Check that PostgreSQL service is added and `DATABASE_URL` environment variable is set
   - Ensure database exists and migrations are applied

2. **Railway Deployment Issues**

   - Verify the repository is connected to Railway
   - Check build logs in Railway dashboard for errors
   - Ensure environment variables are properly configured
   - Review [Railway Setup Guide](railway-setup.md) for troubleshooting steps

3. **Test Failures**

   - Run `dotnet clean` and `dotnet restore`
   - Check that all test dependencies are installed
   - Verify test data setup in test files

4. **Build Errors**
   - Ensure .NET 8.0 SDK is installed
   - Run `dotnet restore` to restore packages
   - Check for missing using statements or references

### Debugging

- Set breakpoints in controllers and services
- Use the debugger in VS Code or Visual Studio
- Check application logs in the console output
- Use Entity Framework logging for database queries

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Follow the existing code style
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Deployment

🚀 **Live Demo**: This application is deployed on [Railway](https://railway.app), a modern application deployment platform that provides:

- **Automatic deployments** from GitHub
- **PostgreSQL database** with automatic backups
- **Zero-configuration** deployment for .NET applications
- **Environment variable** management
- **Custom domains** and SSL certificates
- **Monitoring and logs** dashboard

For deployment instructions, see the [Railway Setup Guide](railway-setup.md) or [DEPLOYMENT.md](DEPLOYMENT.md) for other hosting options.

---

For more information or support, please refer to the project documentation or create an issue in the repository.
