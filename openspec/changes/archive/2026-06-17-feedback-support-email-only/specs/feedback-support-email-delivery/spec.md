## ADDED Requirements

### Requirement: Feedback submission delivers by email only
When a user submits feedback via `POST /api/feedback/submit`, the system SHALL send an email notification to the configured admin address and return a success response. No data SHALL be persisted to the database.

#### Scenario: Valid feedback submitted
- **WHEN** a valid `BllFeedbackResponse` payload is posted to `POST /api/feedback/submit`
- **THEN** the system sends an email to the admin address containing all feedback fields
- **THEN** the system returns HTTP 200 with `{ "message": "Feedback submitted successfully. Thank you!" }`
- **THEN** no record is written to any database table

#### Scenario: Invalid feedback payload rejected
- **WHEN** a payload with a missing required field or rating outside 1–10 is posted to `POST /api/feedback/submit`
- **THEN** the system returns HTTP 400 with error code `VALIDATION_ERROR`
- **THEN** no email is sent

### Requirement: Support message delivers by email only
When a user sends a support message via `POST /api/feedback/support/send-message`, the system SHALL send an email notification to the configured admin address and return a success response. No data SHALL be persisted to the database.

#### Scenario: Valid support message sent
- **WHEN** a valid payload with `senderEmail`, `subject`, and `message` is posted to `POST /api/feedback/support/send-message`
- **THEN** the system sends an email to the admin address containing sender email, subject, and message body
- **THEN** the system returns HTTP 200 with `{ "message": "Message successfully sent!" }`
- **THEN** no record is written to any database table

#### Scenario: Invalid support message payload rejected
- **WHEN** a payload with a missing `senderEmail` or `message` is posted to `POST /api/feedback/support/send-message`
- **THEN** the system returns HTTP 400 with error code `VALIDATION_ERROR`
- **THEN** no email is sent

## REMOVED Requirements

### Requirement: Feedback stored in database
**Reason**: DB persistence of feedback has no consumer once the private admin app is removed; email delivery is sufficient for the open-source release.
**Migration**: No migration path — existing rows are dropped by the `RemoveFeedbackSupportTables` EF migration. Email delivery already duplicated all submission data.

### Requirement: Support messages stored and managed via API
**Reason**: All read and management endpoints (`GET support/messages`, `GET support/unread-count`, `GET support/recent`, `GET support/unresolved`, `GET support/{id}`, `POST support/mark-as-read/{id}`, `POST support/mark-as-resolved/{id}`, `POST support/reply`) were consumed only by the private admin app being removed.
**Migration**: No migration path — existing rows are dropped by the `RemoveFeedbackSupportTables` EF migration. Email delivery already forwarded all incoming messages to the admin address.
