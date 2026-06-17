## MODIFIED Requirements

### Requirement: Employer registration creates an immediately usable account
When an employer submits valid registration data via `POST /api/auth/employer/register`, the system SHALL create the account and return a success response. The account SHALL be immediately usable for login — no email verification step is required.

#### Scenario: Successful registration
- **WHEN** a valid `BllRegister` payload (firstName, lastName, email, password) is posted to `POST /api/auth/employer/register`
- **THEN** the system creates the employer record in the database
- **THEN** the system returns HTTP 200 with a success message
- **THEN** the employer can immediately log in with the registered credentials

#### Scenario: Duplicate email rejected
- **WHEN** an email that already belongs to an existing employer is posted to `POST /api/auth/employer/register`
- **THEN** the system returns HTTP 400 with error code `REGISTRATION_FAILED`
- **THEN** no duplicate employer is created

#### Scenario: Missing required fields rejected
- **WHEN** any of firstName, lastName, email, or password is blank or missing
- **THEN** the system returns HTTP 400 with error code `REGISTRATION_FAILED`

## REMOVED Requirements

### Requirement: Registration sends email verification
**Reason**: Email verification is not needed for self-hosted deployments where the operator controls access.
**Migration**: No migration path — the EmailVerificationTokens table is dropped and the verification email is no longer sent.
