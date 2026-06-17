## Backend setup

Copy `API/appsettings.Example.json` to `API/appsettings.json` and fill in your values:

```json
{
  "Jwt": {
    "SecretKey": "min-32-char-random-secret",
    "Issuer": "ShiftEase",
    "Audience": "ShiftEaseUsers",
    "AccessTokenExpirationMinutes": 15
  },
  "EmailSettings": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "no-reply@example.com",
    "SmtpPassword": "...",
    "FromEmail": "no-reply@example.com"
  },
  "Cors": {
    "Origins": "http://localhost:3000"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=shiftease;Username=postgres;Password=..."
  }
}
```

> **Data Protection note:** Encryption keys for PII fields are stored in the `DataProtectionKeys`
> table in PostgreSQL. The database must be reachable on first start. Do not delete these keys —
> they are required to decrypt existing data.

## Updating migrations

```bash
cd DAL
dotnet ef database drop
dotnet ef migrations remove
dotnet ef migrations add Name_of_migration
dotnet ef database update
```
