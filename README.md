# Mithrandir 🧙‍♂️

API key management and rate limiting service built with .NET, PostgreSQL, Redis and Docker.

Mithrandir is a service that provides API key generation, validation and management with rate limiting and usage
analytics. Currently it supports a Free and Pro tier with configurable rate limits and IP whitelisting. All API 
keys are hashed using BCrypt before storage.

Now that I've finished with the prototype, I'll be working on deployment and building a front end dashboard for interacting 
with the app.

## Tech Stack 👷‍♂️

- **Framework**: .NET 9
- **Database**: PostgreSQL 16 with Entity Framework Core
- **Caching**: Redis (for rate limiting)
- **Security**: BCrypt key hashing
- **Testing**: xUnit integration testing
- **Infrastructure**: Docker

## Features 🚀

**API Key Management**
- Generate API keys with tiers and expiry dates
- Revoke and delete keys 
- Key validation e.g. active, expired, revoked
- Keys stored as BCrypt hashes

**Rate Limiting**
- Sliding 10 minute window rate limit
- Tier based limits e.g. free (10 requests) and pro (50 requests)
- Inclusion of `Retry-After` header in client responses

**IP Whitelisting**
- Per key IP whitelists
- IPv4 and IPv6 address standardisation
- Proxy IP detection using `X-Forwarded-For` header

**Usage Analytics**
- Timestamp, endpoint, IP, and status code logging for requests
- Ability to query key usage e.g. total requests, success/failure counts
- Summary of usage by endpoint and HTTP status code

## API Endpoints

**Public Endpoints** (require `X-Api-Key` header)

| Method | Endpoint | Description          |
|--------|----------|----------------------|
| POST | `/api/keys/validate` | Validate an API key  |
| POST | `/api/keys/usage` | Get usage statistics |
| PATCH | `/api/keys/revoke` | Revoke a key         |

**Admin Endpoints** (require `X-Admin-Key` header)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/admin/keys/generate` | Generate a new API key |
| DELETE | `/api/admin/keys/delete` | Delete an API key |
| POST | `/api/admin/keys/whitelist/add` | Add IP to whitelist |
| DELETE | `/api/admin/keys/whitelist/remove` | Remove IP from whitelist |

## Architecture 

```
src/
├── Controllers/       # API endpoints
├── Services/          # Key management and rate limiting helpers
├── Middleware/        # Authentication, rate limiting, request logging
├── Models/            # Classes and DTOs
├── Data/              # EF Core database context
└── Utilities/         # IP address helpers

tests/
├── mithrandir.Tests/  # Integration tests
```

## Testing 🧪

**Coverage**
- Key generation, validation, revocation and deletion
- IP whitelist add and remove operations
- Rate limit enforcement and window reset
- Usage data requests

**Configuration**
- In-memory Postgres database
- Dedicated test Redis instance 
- Custom `TimeProvider` for testing rate limiting reset

## License 👨‍⚖️

This project is open source under the MIT License.

## Contact 📫

Check out my other projects and contact info on my [GitHub](https://github.com/rohandrummond) profile.

