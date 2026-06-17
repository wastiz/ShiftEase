## MODIFIED Requirements

### Requirement: Employer login succeeds on valid credentials without email gate
When an employer submits valid credentials to `POST /api/auth/employer/login`, the system SHALL authenticate them and return access and refresh tokens. The system SHALL NOT block login based on any email verification status.

#### Scenario: Successful login
- **WHEN** a valid email and password are posted to `POST /api/auth/employer/login`
- **THEN** the system returns HTTP 200 with an access token and refresh token
- **THEN** the employer is authenticated regardless of any prior email verification state

#### Scenario: Invalid credentials rejected
- **WHEN** an incorrect email or password is posted to `POST /api/auth/employer/login`
- **THEN** the system returns HTTP 401 with error code `INVALID_CREDENTIALS`

## REMOVED Requirements

### Requirement: Login blocked for unverified email
**Reason**: Email verification is removed entirely; there is no verified/unverified distinction.
**Migration**: Existing employers with `IsEmailVerified = false` in the database will be able to log in after the migration drops the column and the gate check is removed from code.

### Requirement: Email verification endpoint
**Reason**: The `GET /api/auth/verify-email?token=` endpoint has no purpose once verification is removed.
**Migration**: The endpoint is deleted. The `/verify-email` frontend route is also removed.
