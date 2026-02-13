# Dotnet Web API Starter (Azure-ready)

This repository contains a basic ASP.NET Core Web API project configured for Azure Web App deployment.

## Features

- ASP.NET Core Web API (`net8.0`)
- Swagger / OpenAPI
- Hangfire with in-memory storage
- Hangfire dashboard secured with basic authentication
- Entity Framework Core with SQL Server provider
- `Book` entity with full CRUD API
- FluentValidation for request validation
- `/health` endpoint for liveness checks

## Configuration (important before deploy)

Set these values in Azure Web App **Application Settings**:

- `ConnectionStrings__DefaultConnection`
- `Hangfire__Dashboard__Username`
- `Hangfire__Dashboard__Password`

> Note: Hangfire in-memory storage is volatile (data lost on restart/scale-out). Keep this for development or single-instance non-critical workloads.

## Endpoints

- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`
- `GET /swagger`
- `GET /hangfire` (basic auth protected)
- `GET /health`
