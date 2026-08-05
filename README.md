# TaskTracker Application (Fullstack Pet-Project)

The project is a fully functional Fullstack task management application. It was specifically developed to demonstrate skills in architectural design, Docker integration, database management, and the separation of concerns (Backend / Frontend).

## Core Technology Stack
* **Backend:** .NET 10, ASP.NET Core Web API (Minimal API), Entity Framework Core
* **Database:** SQLite
* **Frontend:** HTML5, CSS3, JavaScript (fully AI-generated using Claude)
* **DevOps / Infrastructure:** Docker, Docker Compose

## Key Architectural Features
* **HTTP Communication:** The Backend API and Frontend are completely isolated, run on different ports, and communicate via HTTP with a properly configured **CORS** policy.
* **Minimal API:** The backend is implemented using ASP.NET Core Minimal API, providing a lightweight and efficient approach to building RESTful services.
* **Layered Architecture:** The application follows a layered architecture, separating concerns into distinct layers: WebApi, Business (Anemic Domain Model) and Data Access, for better maintainability and scalability.
* **Fluent Validation:** The application uses FluentValidation for robust input validation, ensuring that all user inputs are properly checked before processing.
* **FluentResults:** The application employs FluentResults for consistent error handling and result management, providing a clear and structured way to handle success and failure scenarios. In WebApi layer, TypedResults are used to return results with appropriate HTTP status codes.
* **Fluent API Configuration:** The application leverages EF Core's Fluent API for precise and flexible configuration of entity relationships, constraints, and database schema. Code first migrations are used to create and update the database schema based on the defined entity models.
* **Mapster Integration:** The application uses Mapster for efficient object mapping between DTOs and EF Core entities, streamlining data transfer and reducing boilerplate code.
* **Identity Management:** The application implements a custom account management system using default ASP.NET Core Identity for user authentication and authorization, ensuring secure access to resources and personalized user experiences.
* **Sorting/Filtering/Pagination:** The application implements sorting, filtering, and pagination for efficient data retrieval and presentation, enhancing user experience and performance.
* **Automated Data Seeding:** When a new user registers, a welcome dataset (5 categories and 5 tasks) is automatically created within a single transaction, allowing immediate interface evaluation.
* **Data-level CQRS:** The application implements a data-level CQRS pattern, separating read and write operations for improved performance and scalability. WebApi DTOs are used for data transfer, while EF Core entities are utilized for database operations.
* **Global Filtering:** The application implements global filtering for all queries, ensuring that only data associated with the currently authenticated user is accessible, enhancing security and data integrity.
* **Automated Migrations:** The SQLite database is automatically deployed and updated upon the first application launch using EF Core Migrations.
* **Dockerized Environment:** The application is fully containerized, allowing for easy deployment and consistent environments across different machines.
* **Persistent Storage (Docker Volumes):** The SQLite database file is mapped to the host machine, guaranteeing data persistence when containers are restarted.
* **Swagger Integration:** The backend API is fully documented with Swagger, providing an interactive interface for testing and exploring the API endpoints.

## How to Run the Project in a Single Command

To launch the project, you only need **Docker Desktop** installed.

1. Clone the repository:
```bash
   git clone https://github.com/paulSoryu/WebApiTaskTracker.git
   cd WebApiTaskTracker
```

2. Build and start the containers:
```bash
   docker-compose up --build
```

3. Once successfully launched, the applications are available at:
   * **Frontend:** `http://localhost:3000` — user interface.
   * **Backend API (Swagger):** `http://localhost:5001/swagger/index.html` — interactive API documentation.

> **Note:** Ports `3000` and `5001` must be free on your machine.
## How to Test
1. Open the frontend (`http://localhost:3000`) and register a new user.
2. The system will automatically create the account, apply a transaction to generate 5 starting categories and tasks, and log you in.
3. You will immediately see a populated interface, ready for work.