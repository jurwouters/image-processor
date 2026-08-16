# Copilot Instructions

## Project Guidelines
- Prefers using well-known design patterns where possible instead of custom/less common patterns.
- Keep API contracts separate from service-layer models; do not use API contract DTOs as internal service input/output models.
- Prefer API contract names to reflect the endpoint intent; avoid generic names like BatchResponse/BatchInfoResponse when endpoint-specific names are clearer.
- Prefer minimal changes and avoid over-engineering; keep code and scope as small as possible.
- Use central NuGet package version management via props files (Directory.Packages.props) for this repository.
- Prefer Blazor project structure where feature-specific logic is separated from .razor markup to align with clean code principles.
- Prefer user-facing page content that describes semantic user tasks and avoids technical implementation details.

## Docker Guidelines
- Prefer docker-compose variable interpolation using the syntax ${VAR} instead of ${VAR:?message}. 
- Use docker-compose variables without defaults; supply all values via .env instead of using fallback values in compose files.