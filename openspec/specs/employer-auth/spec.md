# Employer Auth Spec

## Purpose

Governs employer authentication: login and token issuance. Email verification is not part of this capability — accounts are usable immediately after registration.

## Requirements

### Requirement: Employer login succeeds on valid credentials without email gate
When an employer submits valid credentials to `POST /api/auth/employer/login`, the system SHALL authenticate them and return access and refresh tokens. The system SHALL NOT block login based on any email verification status.

#### Scenario: Successful login
- **WHEN** a valid email and password are posted to `POST /api/auth/employer/login`
- **THEN** the system returns HTTP 200 with an access token and refresh token
- **THEN** the employer is authenticated regardless of any prior email verification state

#### Scenario: Invalid credentials rejected
- **WHEN** an incorrect email or password is posted to `POST /api/auth/employer/login`
- **THEN** the system returns HTTP 401 with error code `INVALID_CREDENTIALS`
