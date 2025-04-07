# KittySaver

KittySaver is a comprehensive cat adoption management platform designed to connect cat shelters and individuals with potential adopters. The application provides a robust API-driven backend that supports advertisement management, cat profiles, adoption processes, and user authentication.

Live at: [uratujkota.pl](https://uratujkota.pl)

# Why KittySaver?

KittySaver is more than just a technical project - it's a personal mission.

As a .NET developer with two years of commercial experience, I wanted to build something that not only puts all of my backend, frontend, and architectural skills to the test - but also makes a real difference.

The app is currently in its early test phase, open to everyone without advertisement approvals, and using test data only. But in the coming months, I plan to reach out to real people - individuals, shelters, and foundations - who have cats in need of adoption.

I strongly believe feline homelessness is a serious issue. If this application helps even one cat find a safe, loving home - I'll consider it a true success.

Beyond clean code and architecture, KittySaver is built to be a free, solid, and easy-to-maintain platform for anyone who truly cares about animal welfare. Because time is limited, and the need is real.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Configuration](#configuration)
  - [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)

## Overview

KittySaver is designed to simplify the process of cat adoption by providing a centralized platform for shelters and individuals to manage and publish adoption advertisements. The system prioritizes cats based on various factors like health status, age, and behavior to ensure that cats with urgent needs receive greater visibility.

The application follows SOLID and Domain-Driven Design principles to create a maintainable, scalable solution with clear separation of concerns and a rich domain model.

## Architecture
Solution contains three main services - API, Auth API, Blazor WASM Client.

KittySaver API implements Vertical Slices Architecture with the following key components:

- **API Layer:** REST API (Level 3 of Richardson Maturity Model) implemented with use of Vertical Slice Architecture to composite minimal API with CQRS and EF9.
- **Domain Layer:** Core business entities, value objects, and domain services - with strong adhere to DDD principles.
- **Shared Layer:** Layer that contains shared contracts between API's and Blazor front-end application, to ensure consistency within the solution.

### Why This Architecture?

- **Vertical Slice Architecture**: This project uses Vertical Slice Architecture instead of Onion Architecture to optimize development time, reduce the need to jump between layers, and minimize the surface area for bugs. Based on my personal experience, while Onion Architecture has its strengths, it often results in a time-consuming and hard-to-navigate codebase that becomes increasingly difficult to maintain as the project scales.
- **CQRS (Command Query Responsibility Segregation)**: I follow DDD principles for state-changing operations that require maximum consistency and a rich, expressive domain model. For read-only purposes, I use optimized Read Models with dedicated DbContext instances to improve query performance.
- **Mediator Pattern**: I use MediatR to handle commands and queries, as well as manage cross-cutting concerns such as command validation and HATEOAS support. MediatR also lays the groundwork for future domain event handling.
- **Domain-Driven Design**: The project features a rich domain model with encapsulated business rules. I've aimed to follow Eric Evans' principles as closely as possible. Currently, the project contains a single Aggregate - the Person Aggregate. The design allows for easy extension with future aggregates as needed. The Person entity acts as the Aggregate Root, with Cat and Advertisement as entities within the aggregate. I use Value Objects and domain repositories based on a generic repository pattern to encapsulate common operations on aggregate roots.
- **HATEOAS**: Hypermedia as the Engine of Application State is used for RESTful API design. The Blazor front-end is free from hard-coded API endpoint URLs. Instead, API responses include available actions on the resource for the user who triggered the endpoint.

## Key Features

### Authentication and Authorization
- JWT-based authentication with refresh tokens
- Email verification and password reset

### User Management
- User registration and profile management
- Personalized user dashboard

### Cat Management
- The first thing to do after successfully logging into the system is to create your cats. You can define the cats that are available for adoption. Each cat can have attributes such as health status, urgency for veterinary assistance, behavior, and age. You can also provide a detailed description of the cat and its needs. Upload as many gallery images as you'd like, and choose one image to be used as the thumbnail.
- Cats are private by default after creation. To make them publicly visible, you need to create an advertisement for each cat or group of cats.

### Advertisement Management
- Once you've created your cat, you can make it publicly visible by creating an advertisement.
- If a cat can be adopted individually, you can create a separate advertisement for that single cat.
- If you have bonded cats that should only be adopted together, you can include them all in one advertisement. This clearly signals to users that the cats are only available as a group.
- If a cat from an active advertisement becomes unavailable for any reason, you can remove it from the advertisement. You can then either delete the cat or move it to a different advertisement. All cat properties are preserved after removal - this is the main benefit of managing cats independently from advertisements.
- After a successful adoption, you can close the advertisement. All cats assigned to that advertisement will be marked as adopted and cannot be reused in the system.
- Advertisements can have their own optional description. You can use it to describe the relationship between cats in a group, or to provide any additional context about the adoption.

### Discovery
- Search and filtering capabilities
- Prioritized listings based on cat needs
- Public and authenticated access modes

## Tech Stack

### Backend
- **.NET 9**: Core framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: ORM for data access
- **SQL Server**: Database
- **MediatR**: Mediator pattern implementation
- **FluentValidation**: Commands validation
- **Mapperly**: Source generated object mapping
- **Swagger/OpenAPI**: API documentation

### Frontend
- **Blazor WebAssembly**: Core framework

### Tests
- **xUnit**: Testing framework - both for unit tests and integration tests.

## Project Structure

The solution is organized into the following projects:

- **KittySaver.Api**: Main API project with endpoints and application logic (VSA)
- **KittySaver.Domain**: Domain entities, aggregates, and business logic (DDD)
- **KittySaver.Shared**: Shared DTOs, and utilities
- **KittySaver.Api.Tests**: API tests (unit and integration)
- **KittySaver.Auth.Api**: Authentication and authorization API
- **KittySaver.Auth.Api.Tests**: Authentication API tests (unit and integration)
- **KittySaver.Wasm**: Blazor WASM frontend client.
- **KittySaver.Aspire**: .NET Aspire support for Development environment.

Key folders within the API project:

```
src/
├── KittySaver.Api/
│   ├── Behaviours/            # MediatR pipeline behaviors
│   ├── Exceptions/            # Exception handlers
│   ├── Features/              # Feature-organized endpoints and handlers
│   │   ├── Advertisements/    # Advertisement-related features
│   │   ├── ApiDiscovery/      # API discovery endpoints
│   │   ├── Cats/              # Cat-related features
│   │   └── Persons/           # Person-related features
│   ├── Hateoas/               # HATEOAS implementation
│   ├── Infrastructure/        # Infrastructure services
│   │   ├── Clients/           # External API clients
│   │   ├── Endpoints/         # Endpoint registration
│   │   └── Services/          # Application services
│   └── Persistence/           # Data access and ORM configuration
├── KittySaver.Auth.Api/       # Authentication API
├── KittySaver.Domain/         # Domain model
└── KittySaver.Shared/         # Shared components
```

## Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server (or SQL Server Express)
- Visual Studio 2022, JetBrains Rider, or VS Code
- Git

### Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/kittysaver.git
cd kittysaver
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

### Configuration

1. Configure the database connection in `appsettings.json` in both API projects:

```json
{
  "ConnectionStrings": {
    "Database": "Server=(localdb)\\mssqllocaldb;Database=KittySaverDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

2. Configure JWT settings in `appsettings.json` in the Auth.API project:

```json
{
  "AppSettings": {
    "Token": "your-secret-key-at-least-16-characters-long",
    "MinutesTokenExpiresIn": "60"
  }
}
```

3. Configure Email settings in `appsettings.json` in the Auth.API project:

```json
{
  "EmailSettings": {
    "Server": "smtp.example.com",
    "Port": 587,
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "KittySaver",
    "Username": "smtp_username",
    "Password": "smtp_password",
    "UseSsl": true,
    "WebsiteBaseUrl": "https://yourdomain.com"
  }
}
```

### Running the Application

1. Apply database migrations:
```bash
cd src/KittySaver.Api
dotnet ef database update

cd ../KittySaver.Auth.Api
dotnet ef database update
```

2. Run the API:
```bash
# Run the main API
cd src/KittySaver.Api
dotnet run

# Run the Auth API
cd src/KittySaver.Auth.Api
dotnet run
```

3. Access the APIs:
   - Main API: https://localhost:7127 (or http://localhost:5181)
   - Auth API: https://localhost:7124 (or http://localhost:5113)
   - Swagger UI: https://localhost:7127/swagger (for Main API)
   - Swagger UI: https://localhost:7124/swagger (for Auth API)

## API Documentation

The API is documented using Swagger/OpenAPI. When running the application, you can access the Swagger UI at:

- Main API: https://localhost:7127/swagger
- Auth API: https://localhost:7124/swagger


### Authentication API

- `POST /api/v1/application-users/register` - Register a new user
- `POST /api/v1/application-users/login` - Login and receive JWT token
- `POST /api/v1/application-users/refresh-token` - Refresh an expired JWT token
- `POST /api/v1/application-users/logout` - Logout and invalidate tokens
- `POST /api/v1/application-users/forgot-password` - Request password reset
- `POST /api/v1/application-users/reset-password` - Reset password
- `GET /api/v1/application-users/me` - Get current user information

### Main API

The API follows RESTful design principles and implements HATEOAS for resource discovery.
Thanks to the use of CQRS and Domain-Driven Design, the REST API structure is aligned with the aggregate hierarchy, making the endpoints consistent with the domain model. This improves clarity, maintainability, and makes the API more intuitive for consumers.

- **Discovery endpoint**
  - `GET /api/v1/`

- **Persons**
  - `POST /api/v1/persons` - Create a new person
  - `GET /api/v1/persons/{id}` - Get a specific person
  - `PUT /api/v1/persons/{id}` - Update a person
  - `DELETE /api/v1/persons/{id}` - Delete a person

- **Cats**
  - `GET /api/v1/persons/{personId}/cats` - Get cats for a person
  - `POST /api/v1/persons/{personId}/cats` - Create a new cat
  - `GET /api/v1/persons/{personId}/cats/{id}` - Get a specific cat
  - Additional endpoints for cat thumbnail, gallery, and management

- **Advertisements**
  - `GET /api/v1/advertisements` - Get all public advertisements
  - `GET /api/v1/advertisements/{id}` - Get a specific public advertisement
  - `GET /api/v1/persons/{personId}/advertisements` - Get advertisements for a person
  - `POST /api/v1/persons/{personId}/advertisements` - Create a new advertisement
  - Additional endpoints for updating, deleting, and managing advertisement status


## Testing

The project includes almost 300 comprehensive tests covering both unit tests and integration tests for the API and Auth API components.

To run the tests:

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test ./tests/KittySaver.Api.Tests
dotnet test ./tests/KittySaver.Auth.Api.Tests
```

### Test Categories

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test interactions between components
- **API Tests**: Test API endpoints and responses

## Deployment

The application is currently deployed at [uratujkota.pl](https://uratujkota.pl).

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

© 2025 KittySaver - Artur Koniec. All rights reserved.
