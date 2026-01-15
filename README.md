# Mithrandir 🔮

<!-- TODO: Add screenshot of dashboard here -->
<!-- ![Dashboard Screenshot](screenshot.png) -->

  🔗 [Live demo](https://mithrandir-rho.vercel.app/)

**About the project**

Mithrandir is a service that supports API key management, IP whitelisting and rate limiting. 

It's built around a .NET MVC API that uses PostgreSQL for securely storing API keys, and Redis for monitoring rate limits. Also included is a Next.js dashboard for interacting with the service using a browser.

The .NET backend uses a custom middleware pipeline to process each request through logging, authentication and rate limiting before reaching the controllers. Once a request has been authenticated, Entity Framework Core handles database access and a response is sent. 

## Key features 💡

**.NET Architecture**

- MVC pattern with controllers, services, and models
- Custom middleware pipeline i.e. logging → auth → rate limit → controller
- Service layer with dependency injection (`IApiKeyService`, `IRateLimitService`)
- Entity Framework Core for PostgreSQL data access


**API Key Management**

- Cryptographically secure key generation using `RandomNumberGenerator`
- Keys stored as BCrypt hashes
- Admin ability to generate, validate, revoke and delete keys
- Optional expiration dates and usage tracking

**API Key Authentication**

- Dual authentication approach - `X-Api-Key` for users, `X-Admin-Key` for admin endpoints
- Timing attack prevention via constant time comparison
- IP whitelisting with IPv4/IPv6 normalization

**Rate Limiting**

- Redis-based sliding window rate limit
- Tiered limits with `Retry-After` header on 429 responses

**Testing**

- xUnit integration tests with real middleware pipeline
- In-memory database and dedicated Redis instance for testing
- Custom `FakeTimeProvider` for testing time-dependent rate limit windows

## Tech stack ⚙️

**Backend**

- .NET
- PostgreSQL
- Redis
- Entity Framework Core
- BCrypt
- xUnit

**Frontend**

- Next.js
- Tailwind CSS
- shadcn

**Infrastructure**

- Amazon EC2
- Docker Compose 
- Vercel
- Terraform
- GitHub Actions

## Deployment ☁️

The .NET  API, PostgreSQL and Redis run as a Docker Compose multi-container application on Amazon EC2. The Next.js dashboard is hosted with Vercel.

Terraform provisions the AWS infrastructure using code i.e. EC2 instance with Docker, VPC, ECR , IAM roles and security groups.

GitHub Actions handles CI/CD which includes through 3 phases:

1. Set up infrastructure and run tests on an Ubuntu GitHub runner
2. After tests pass, a Docker image is built for the .NET API and pushed to Amazon ECR
3. Amazon EC2 is accessed using SSH and a script runs to build the latest image and restarts the application with Docker Compose

## Getting Started 🚀

**Prerequisites**

- .NET SDK (if running tests)
- Node.js
- Docker & Docker Compose

**Clone the repository**

```bash
git clone https://github.com/rohandrummond/mithrandir.git
cd mithrandir
```

**Set up environment Variables**

Create a `.env` file in the root directory:

```
POSTGRES_PASSWORD=your-password
POSTGRES_USER=your-username
POSTGRES_DB=mithrandirdb
ADMIN_API_KEY=your-admin-key
```

Create a `.env.local` file in the `/dashboard` directory:

```
NEXT_PUBLIC_DOTNET_API_URL=http://localhost:8080
ADMIN_API_KEY=your-admin-key
```

**Run the project**

```bash
# Start the backend (Docker Compose builds and runs API, PostgreSQL, and Redis containers)
docker compose up -d

# Start the frontend
cd dashboard
npm install
npm run dev
```

The API runs on `http://localhost:8080` and the dashboard on `http://localhost:3000`.

**Running Tests**

```bash
dotnet test
```