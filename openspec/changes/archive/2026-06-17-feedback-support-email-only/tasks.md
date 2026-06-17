## 1. Remove DAL layer (Domain, repositories, DTOs)

- [x] 1.1 Delete `backend/Domain/FeedbackResponse.cs`
- [x] 1.2 Delete `backend/Domain/SupportMessage.cs`
- [x] 1.3 Delete `backend/DAL.Contracts/IFeedbackRepository.cs`
- [x] 1.4 Delete `backend/DAL.Contracts/ISupportRepository.cs`
- [x] 1.5 Delete `backend/DAL/Repositories/FeedbackRepository.cs`
- [x] 1.6 Delete `backend/DAL/Repositories/SupportRepository.cs`
- [x] 1.7 Delete `backend/DAL.DTO/FeedbackDtos/DalFeedbackResponse.cs`
- [x] 1.8 Delete `backend/DAL.DTO/SupportDtos/DalSupportMessage.cs`
- [x] 1.9 Delete `backend/DAL.DTO/SupportDtos/DalSupportReply.cs`
- [x] 1.10 Delete `backend/BLL.DTO/SupportDtos/BllSupportMessage.cs`
- [x] 1.11 Delete `backend/BLL.DTO/SupportDtos/BllSupportReply.cs`

## 2. Update AppDbContext and remove repository registrations

- [x] 2.1 In `backend/DAL/AppDbContext.cs`, remove `public DbSet<FeedbackResponse> FeedbackResponses { get; set; }` and `public DbSet<SupportMessage> SupportMessages { get; set; }` and any related `using` imports
- [x] 2.2 In `backend/DAL/Migrations/AppDbContextModelSnapshot.cs`, remove the `FeedbackResponse` and `SupportMessage` entity configurations
- [x] 2.3 In `backend/API/Program.cs`, remove `IFeedbackRepository`/`FeedbackRepository` and `ISupportRepository`/`SupportRepository` service registrations and related `using` imports

## 3. Update IMailService signature and MailService implementation

- [x] 3.1 In `backend/BLL.Contracts/IMailService.cs`, change `SendSupportNotificationAsync(DalSupportMessage message)` to `SendSupportNotificationAsync(string senderEmail, string subject, string message)` and remove the `using DTOs.SupportDtos;` import
- [x] 3.2 In `backend/BLL/Services/MailService.cs`, update `SendSupportNotificationAsync` signature to `(string senderEmail, string subject, string message)`, replace `message.SenderEmail` / `message.Subject` / `message.Message` with the new parameters, and remove the `using DTOs.SupportDtos;` import

## 4. Simplify BLL contracts and services

- [x] 4.1 In `backend/BLL.Contracts/IFeedbackService.cs`, remove the `GetAllFeedbackAsync()` method and remove `using Domain;` if no longer needed
- [x] 4.2 In `backend/BLL.Contracts/ISupportService.cs`, remove all methods except `SendMessageAsync` (remove `GetAllMessagesAsync`, `GetUnreadCountAsync`, `GetRecentMessagesAsync`, `GetOrganizationsWithIssuesAsync`, `GetByIdAsync`, `MarkAsReadAsync`, `MarkAsResolvedAsync`, `ReplyToMessageAsync`) and remove `using Domain;` if no longer needed
- [x] 4.3 In `backend/BLL/Services/FeedbackService.cs`, remove `_repository` field, remove `IFeedbackRepository repository` constructor parameter, remove the `DalFeedbackResponse` mapping block, replace the `SubmitFeedbackAsync` body with just `await _mailService.SendFeedbackNotificationAsync(dto);`, and delete the `GetAllFeedbackAsync` method; remove `using DAL.Contracts;` and `using DTOs.FeedbackDtos;` (DAL.DTO) imports
- [x] 4.4 In `backend/BLL/Services/SupportService.cs`, remove `_repository` field, remove `ISupportRepository repository` constructor parameter, replace the `SendMessageAsync` body with just `await _mailService.SendSupportNotificationAsync(dto.SenderEmail, dto.Subject, dto.Message);`, and delete all other method implementations; remove `using DAL.Contracts;`, `using Domain;`, and `using DTOs.SupportDtos;` (DAL.DTO) imports

## 5. Slim down FeedbackController

- [x] 5.1 In `backend/API/Controllers/FeedbackController.cs`, remove the `ISupportService` constructor injection that is no longer needed for admin reads (keep it only for `SendMessageAsync`; verify it is still needed for the send endpoint)
- [x] 5.2 Remove `GET /api/feedback` (`GetAll`) endpoint
- [x] 5.3 Remove `GET /api/feedback/support/messages` (`GetAllMessages`) endpoint
- [x] 5.4 Remove `GET /api/feedback/support/unread-count` (`GetUnreadCount`) endpoint
- [x] 5.5 Remove `GET /api/feedback/support/recent` (`GetRecentMessages`) endpoint
- [x] 5.6 Remove `GET /api/feedback/support/unresolved` (`GetUnresolvedMessages`) endpoint
- [x] 5.7 Remove `GET /api/feedback/support/{id}` (`GetMessageById`) endpoint
- [x] 5.8 Remove `POST /api/feedback/support/mark-as-read/{id}` (`MarkAsRead`) endpoint
- [x] 5.9 Remove `POST /api/feedback/support/mark-as-resolved/{id}` (`MarkAsResolved`) endpoint
- [x] 5.10 Remove `POST /api/feedback/support/reply` (`ReplyToMessage`) endpoint
- [x] 5.11 Remove `using DTOs.SupportDtos;` (DAL.DTO) import from `FeedbackController.cs`; update remaining `using` imports as needed

## 6. Add cleanup EF migration

- [x] 6.1 From `backend/DAL/`, run `dotnet ef migrations add RemoveFeedbackSupportTables` to generate a new migration file and update the model snapshot
- [x] 6.2 Edit the generated migration's `Up()` to contain only:
  ```csharp
  migrationBuilder.Sql("DROP TABLE IF EXISTS \"FeedbackResponses\";");
  migrationBuilder.Sql("DROP TABLE IF EXISTS \"SupportMessages\";");
  ```
  and leave `Down()` empty

## 7. Verification

- [x] 7.1 Run `dotnet build` from `backend/` — confirm zero errors and no remaining references to `IFeedbackRepository`, `ISupportRepository`, `FeedbackResponse`, `SupportMessage`, `DalFeedbackResponse`, `DalSupportMessage`, or `DalSupportReply`
- [x] 7.2 Run `dotnet test` from `backend/` — confirm all tests pass
- [x] 7.3 Confirm `dotnet ef migrations list` (from `backend/DAL/`) shows no pending migrations other than `RemoveFeedbackSupportTables`
