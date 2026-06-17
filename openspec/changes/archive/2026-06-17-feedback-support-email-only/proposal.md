## Why

Feedback submissions and support tickets were built for a hosted service where the maintainer managed them through a private admin dashboard backed by PostgreSQL. For the open-source release there is no admin dashboard, so the DB tables and all read/management endpoints are dead weight that complicates self-hosting without providing any value.

## What Changes

- **BREAKING** — Remove `Domain/FeedbackResponse.cs` and `Domain/SupportMessage.cs`; drop the `FeedbackResponses` and `SupportMessages` DB tables via a cleanup migration
- **BREAKING** — Delete `DAL.Contracts/IFeedbackRepository.cs`, `DAL.Contracts/ISupportRepository.cs`, `DAL/Repositories/FeedbackRepository.cs`, `DAL/Repositories/SupportRepository.cs`
- **BREAKING** — Delete DAL DTOs: `DAL.DTO/FeedbackDtos/DalFeedbackResponse.cs`, `DAL.DTO/SupportDtos/DalSupportMessage.cs`, `DAL.DTO/SupportDtos/DalSupportReply.cs`
- **BREAKING** — Delete BLL DTOs no longer needed after repository removal: `BLL.DTO/SupportDtos/BllSupportMessage.cs`, `BLL.DTO/SupportDtos/BllSupportReply.cs`
- **BREAKING** — Remove `IFeedbackRepository` and `ISupportRepository` registrations from `Program.cs`
- Remove `DbSet<FeedbackResponse>` and `DbSet<SupportMessage>` from `AppDbContext`
- Simplify `IFeedbackService` / `FeedbackService`: strip `GetAllFeedbackAsync` and the repository dependency; `SubmitFeedbackAsync` sends email only
- Simplify `ISupportService` / `SupportService`: strip all read/management methods and the repository dependency; `SendMessageAsync` sends email only
- Remove all admin-facing API endpoints from `FeedbackController`: `GET /api/feedback`, `GET support/messages`, `GET support/unread-count`, `GET support/recent`, `GET support/unresolved`, `GET support/{id}`, `POST support/mark-as-read/{id}`, `POST support/mark-as-resolved/{id}`, `POST support/reply`
- Update `IMailService.SendSupportNotificationAsync` parameter type: replace `DalSupportMessage` (deleted) with the minimal fields needed (inline or a new simple DTO)

## Capabilities

### New Capabilities

_None — this is a pure simplification of existing behaviour._

### Modified Capabilities

- `feedback-support-email-delivery`: Feedback and support tickets are now delivered exclusively by email to the configured admin address (`valnos04@gmail.com`). DB persistence and admin management endpoints are removed entirely.

## Impact

- **Backend files deleted**: `Domain/FeedbackResponse.cs`, `Domain/SupportMessage.cs`, `DAL.Contracts/IFeedbackRepository.cs`, `DAL.Contracts/ISupportRepository.cs`, `DAL/Repositories/FeedbackRepository.cs`, `DAL/Repositories/SupportRepository.cs`, `DAL.DTO/FeedbackDtos/DalFeedbackResponse.cs`, `DAL.DTO/SupportDtos/DalSupportMessage.cs`, `DAL.DTO/SupportDtos/DalSupportReply.cs`, `BLL.DTO/SupportDtos/BllSupportMessage.cs`, `BLL.DTO/SupportDtos/BllSupportReply.cs`
- **Backend files modified**: `DAL/AppDbContext.cs`, `BLL.Contracts/IFeedbackService.cs`, `BLL.Contracts/ISupportService.cs`, `BLL.Contracts/IMailService.cs`, `BLL/Services/FeedbackService.cs`, `BLL/Services/SupportService.cs`, `API/Controllers/FeedbackController.cs`, `API/Program.cs`
- **Database**: `FeedbackResponses` and `SupportMessages` tables are no longer created by migrations; a cleanup migration drops them for existing deployments
- **API surface**: `POST /api/feedback/submit` and `POST /api/feedback/support/send-message` remain; all read/management endpoints are removed
- **No frontend changes**: the frontend only calls the two POST endpoints that are kept
- **Email delivery unchanged**: `MailService.AdminEmail` is already `valnos04@gmail.com`; SMTP config in `appsettings.json` is unchanged
