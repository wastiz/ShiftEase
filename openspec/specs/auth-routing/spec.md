# Spec: Auth Routing

## Purpose

Defines which routes the authentication middleware treats as publicly accessible (no login required) and how protected routes are guarded.

## Requirements

### Requirement: Public route list excludes root path
The middleware public-route list SHALL contain `/sign-in`, `/reset-password`, and `/onboarding`. The path `/` SHALL NOT appear as a standalone public route; it is handled exclusively as a redirect source to `/sign-in`.

#### Scenario: Sign-in page is accessible without authentication
- **WHEN** an unauthenticated user navigates to `/sign-in`
- **THEN** the page MUST render normally without redirection

#### Scenario: Reset-password page is accessible without authentication
- **WHEN** an unauthenticated user navigates to `/reset-password`
- **THEN** the page MUST render normally without redirection

#### Scenario: Onboarding page is accessible without authentication
- **WHEN** an unauthenticated user navigates to `/onboarding`
- **THEN** the page MUST render normally without redirection

#### Scenario: Protected routes still require authentication
- **WHEN** an unauthenticated user navigates to any employer or employee route
- **THEN** the middleware MUST redirect to `/sign-in`
