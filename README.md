# ShiftEase

Open-source shift scheduling software. Managers create organisations, departments, and employees, then generate and publish work schedules. Employees view their shifts, submit time-off requests, and set preferences.

**Stack:** .NET 9 · PostgreSQL · Next.js 15 · Tailwind CSS · shadcn/ui

---

## Self-hosting with Docker Compose

The quickest way to run ShiftEase is with Docker Compose.

### 1. Clone the repository

```bash
git clone https://github.com/your-org/ShiftEase.git
cd ShiftEase
```

### 2. Configure environment variables

```bash
cp .env.example .env
```

Open `.env` and set at minimum:

| Variable | Description |
|---|---|
| `POSTGRES_PASSWORD` | Any strong password for the database |
| `JWT_SECRET_KEY` | Random string, ≥ 32 characters |
| `CORS_ORIGINS` | URL the frontend is served from (default `http://localhost:3000`) |

Email settings are optional. If left blank, password-reset tokens are still generated but emails are not sent.

### 3. Start the stack

```bash
docker-compose up --build
```

- Frontend → **http://localhost:3000**
- Backend / Swagger → **http://localhost:8080**

The database schema is applied automatically on first start.

### 4. Create your account

Open http://localhost:3000/sign-in and register as an Employer to get started.

---

## Updating

```bash
git pull
docker-compose up --build
```

Migrations are applied automatically on startup.

---

## Configuration reference

All backend settings are passed as environment variables in `docker-compose.yml` and read by ASP.NET Core using the double-underscore separator convention (e.g. `Jwt__SecretKey` maps to `Jwt.SecretKey` in `appsettings.json`).

### Data Protection keys

Encryption keys for sensitive fields (phone numbers, hourly rates) are stored in the `DataProtectionKeys` table in PostgreSQL. **Do not delete this table or its rows** — doing so will permanently corrupt encrypted data. Back up the table before any destructive database operation.

### Deploying on a custom domain

If you expose the backend on a domain or port other than `localhost:8080`, set `PUBLIC_API_URL` in your `.env` to the full URL reachable from users' browsers, then rebuild:

```bash
PUBLIC_API_URL=https://api.yourcompany.com/api docker-compose up --build
```

---

## Manual setup (without Docker)

See [`backend/README.md`](backend/README.md) for backend setup and [`frontend/.env.example`](frontend/.env.example) for frontend environment variables.

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

AGPL-3.0
