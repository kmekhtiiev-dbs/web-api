# Dotnet Web API Starter (Azure-ready)

This repository contains a basic ASP.NET Core Web API project configured for Azure Web App deployment.

## Features

- ASP.NET Core Web API (`net10.0`)
- Swagger / OpenAPI
- Hangfire with in-memory storage
- Hangfire dashboard secured with basic authentication
- Entity Framework Core with SQL Server provider
- `Book` entity with full CRUD API
- FluentValidation for request validation

## Configuration

Update these settings before deployment:

- `ConnectionStrings:DefaultConnection`
- `Hangfire:Dashboard:Username`
- `Hangfire:Dashboard:Password`

For Azure Web App, set these as Application Settings (environment variables).

## Endpoints

- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `DELETE /api/books/{id}`
- `GET /swagger` (development)
- `GET /hangfire` (basic auth protected)
