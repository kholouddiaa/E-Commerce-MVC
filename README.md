# E-Commerce MVC

An ASP.NET Core MVC E-Commerce application built with a clean **N-Tier Architecture** approach, focusing on separation of concerns, maintainable code structure, Repository Pattern, Unit of Work, DTOs, AutoMapper, and Dependency Injection.

## Project Overview

This project transforms a simple ASP.NET Core MVC application into a structured multi-layer architecture.

The main goal is to apply professional software architecture principles and create a scalable foundation for an E-Commerce system.

---

# Solution Architecture

The project follows **N-Tier Architecture** with clear separation between layers:

```
E-Commerce MVC
│
├── ECommerce.Web              → Presentation Layer (MVC)
│
├── ECommerce.BLL              → Business Logic Layer
│
└── ECommerce.DAL              → Data Access Layer
```

---

## Presentation Layer (ECommerce.Web)

Responsible for:

* Handling HTTP requests and responses
* MVC Controllers and Views
* User interface rendering
* Request validation

Controllers communicate only with the Business Layer and do not access the database directly.

---

## Business Logic Layer (ECommerce.BLL)

Responsible for:

* Application business rules
* Data processing
* Validation logic
* Communication between Presentation and Data Access layers

Implemented services:

* Product Service
* Category Service

---

## Data Access Layer (ECommerce.DAL)

Responsible for:

* Database communication
* Entity Framework Core configuration
* Repository implementations
* Unit of Work pattern

---

# Features Implemented

## Repository Pattern

Implemented repositories to encapsulate database operations.

### Generic Repository

A reusable generic repository was created to reduce duplicated CRUD operations.

Supported operations:

* Get All
* Get By Id
* Add
* Update
* Delete
* Exists

### Specific Repositories

Implemented domain-specific repositories:

* Product Repository
* Category Repository

Custom operations include:

* Retrieving products with category information
* Checking category usage before deletion

---

# Unit of Work Pattern

Implemented Unit of Work to coordinate repository operations.

Responsibilities:

* Expose all repositories
* Manage database operations
* Save pending changes through a single transaction

Structure:

```
UnitOfWork
│
├── ProductRepository
│
├── CategoryRepository
│
└── SaveChangesAsync()
```

---

# DTOs & AutoMapper

DTOs are used to separate database entities from presentation models.

Implemented DTOs for:

* Product
* Category

Benefits:

* Better data security
* Cleaner data transfer
* Separation between layers

AutoMapper is configured for:

* Entity to DTO mapping
* DTO to Entity mapping

---

# Dependency Injection

All repositories and services are registered using ASP.NET Core Dependency Injection.

Implemented service lifetime:

* Scoped services for database-related operations

Registration is handled through extension methods to keep `Program.cs` clean.

---

# Database

Technologies:

* SQL Server
* Entity Framework Core

Database includes:

* Products table
* Categories table
* Entity Framework Migration History

---

# Project Structure

```
ECommerce
│
├── ECommerce.Web
│   ├── Controllers
│   ├── Views
│   ├── Models
│   └── Program.cs
│
├── ECommerce.BLL
│   ├── Services
│   ├── DTOs
│   ├── Interfaces
│   └── Mapping Profiles
│
└── ECommerce.DAL
    ├── Data
    ├── Entities
    ├── Repositories
    ├── UnitOfWork
    └── Interfaces
```

---

# Technologies Used

* ASP.NET Core MVC
* C#
* Entity Framework Core
* SQL Server
* LINQ
* Repository Pattern
* Unit of Work Pattern
* DTO Pattern
* AutoMapper
* Dependency Injection
* Bootstrap

---

# Getting Started

## Requirements

* .NET SDK
* SQL Server
* Visual Studio 2022

## Installation

Clone the repository:

```bash
git clone https://github.com/kholouddiaa/E-Commerce-MVC.git
```

Open the solution:

```
ECommerce.sln
```

Update the connection string inside:

```
appsettings.json
```

Apply migrations:

```bash
Update-Database
```

Run the project:

```bash
dotnet run
```

---

# Authentication & Authorization

Authentication and Authorization using ASP.NET Core Identity were planned as part of the sprint requirements.

The current submitted version focuses on:

* N-Tier Architecture
* Repository Pattern
* Unit of Work
* DTOs
* AutoMapper
* Dependency Injection
* Business Logic Separation

Future improvements include:

* ASP.NET Core Identity integration
* User registration and login
* Role-based authorization
* Admin and Customer roles
* User management dashboard

---

# Future Improvements

* Implement ASP.NET Core Identity
* Add Admin and Customer roles
* Add authorization policies
* Add shopping cart
* Add order management
* Add product search and filtering
* Add pagination
* Add global exception handling
* Add logging

---

# License

This project is created for educational and training purposes.
