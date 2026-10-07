# Project Guidelines

## Documentation
- Whenever a new feature is added or behavior changes, update the relevant README.md (root, `frontend/README.md`, `backend/README.md`, or `frontend/projects/shared/README.md`) in the same change, not as a follow-up.

## Testing
- Keep unit test coverage above 90% at all times. Do not land changes that drop coverage below this threshold; add or update tests alongside code changes rather than after.

## API collections (Insomnia)
- `docs/insomnia-develop.json` (local, http://localhost:5080, DevBypass) and `docs/insomnia-prod.json` (API Gateway + Cognito bearer token) must always mirror the API.
- Whenever a change affects the API (new/removed/renamed endpoint, route, HTTP method, request/response contract, validation, auth, base URL), update BOTH files in the same change.
