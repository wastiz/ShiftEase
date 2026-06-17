# Spec: Root Redirect

## Purpose

Defines the behaviour of the application when a request arrives at the root URL (`/`). In the open-source self-hosted edition there is no landing page; the root URL redirects immediately to `/sign-in`.

## Requirements

### Requirement: Root URL redirects to sign-in
The system SHALL redirect any request to `/` to `/sign-in` with a 307 status code. No landing page content SHALL be rendered.

#### Scenario: Unauthenticated user visits root
- **WHEN** an unauthenticated user navigates to `/`
- **THEN** the browser MUST be redirected to `/sign-in`

#### Scenario: Authenticated user visits root
- **WHEN** an authenticated user navigates to `/`
- **THEN** the middleware MUST redirect to `/sign-in`, which in turn redirects to the user's home route based on role (employer → `/dashboard`, employee → `/my-shifts`)

#### Scenario: Root URL with trailing query string
- **WHEN** a request arrives at `/?foo=bar`
- **THEN** the system MUST redirect to `/sign-in` (query string is not forwarded)
