# Contributing to ShiftEase

Thank you for your interest in contributing! Here is everything you need to get started.

## Project structure

```
ShiftEaseOpenSource/
├── backend/   .NET 9 ASP.NET Core Web API
└── frontend/  Next.js 15 (App Router)
```

## Prerequisites

- **Backend:** .NET 9 SDK, PostgreSQL 16
- **Frontend:** Node.js 22, npm
- **Or:** Docker + Docker Compose (recommended)

## Running locally

### Option A — Docker Compose (easiest)

```bash
cp .env.example .env          # fill in secrets
docker-compose up --build
```

Frontend → http://localhost:3000  
Backend / Swagger → http://localhost:8080

### Option B — manual

**Backend**

```bash
cd backend
cp API/appsettings.Example.json API/appsettings.json
# edit API/appsettings.json with your DB connection and JWT secret
dotnet run --project API
```

**Frontend**

```bash
cd frontend
cp .env.example .env.local
# edit .env.local — set NEXT_PUBLIC_API_URL=http://localhost:3007/api
npm install
npm run dev
```

## Architecture

The backend uses strict layering:

```
Domain → DAL.Contracts / DAL / DAL.DTO →
         BLL.Contracts / BLL.DTO / BLL →
         API.DTO / API
```

- Add domain entities in `Domain/`.
- Add repository interfaces in `DAL.Contracts/`, implementations in `DAL/Repositories/`.
- Add service interfaces in `BLL.Contracts/`, implementations in `BLL/Services/`.
- Add controllers in `API/Controllers/`.

## Database migrations

```bash
cd backend/DAL
dotnet ef migrations add <MigrationName> --startup-project ../API
dotnet ef database update --startup-project ../API
```

## Code style

- **Backend:** PascalCase everywhere; nullable enable; scoped DI lifetime for all repos and services.
- **Frontend:** Strict TypeScript (no `any`); PascalCase component files; camelCase hooks prefixed with `use`.

## Submitting a pull request

1. Fork the repository and create a branch from `main`.
2. Make your changes with focused, small commits.
3. Open a pull request with a clear description of what changed and why.

## Reporting bugs

Open a GitHub issue with:
- Steps to reproduce
- Expected vs. actual behavior
- Backend version / browser / OS
