# User Management REST API

A RESTful user management API built with **ASP.NET Core**, developed and progressively hardened
across three stages: core CRUD, then reliability, then a custom middleware pipeline.

## What it does

Exposes CRUD endpoints for managing users, with input validation, centralised error handling and
token-based authentication implemented as custom middleware.

## Tech stack

C# | .NET | ASP.NET Core | REST | Custom Middleware | Token-Based Authentication | Postman

## Endpoints

| Method | Route | Description |
| ------ | ----- | ----------- |
| GET | `/api/users` | List all users |
| GET | `/api/users/{id}` | Get a single user by id |
| POST | `/api/users` | Create a user |
| PUT | `/api/users/{id}` | Update an existing user |
| DELETE | `/api/users/{id}` | Delete a user |

## Middleware pipeline

Requests pass through custom middleware in a deliberate order:

1. **Request/response logging** - records method, path and status code for every request.
2. **Centralised error handling** - catches unhandled exceptions and returns a consistent error
   response instead of leaking a stack trace.
3. **Token authentication** - rejects requests without a valid token before they reach a controller.

Ordering matters: error handling wraps the pipeline so failures in authentication are still logged
and returned consistently.

## Running locally

```bash
git clone https://github.com/SergioMarinheiro/.NET-User-Management-API.git
cd .NET-User-Management-API
dotnet restore
dotnet run
```

The API starts on the port configured in `Properties/launchSettings.json`.

## Testing

Endpoints were verified with **Postman**. `UserManagementAPI.http` contains sample requests that can
be run directly from Visual Studio or VS Code.

## Notes

Development used **Microsoft Copilot** for code generation and debugging; all generated code was
reviewed, adapted and tested before being adopted.

