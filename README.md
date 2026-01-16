# Mithrandir 🔮

  🔗 [Live demo](https://mithrandir-rho.vercel.app/)

**About the project**

Mithrandir is a service that supports API key management, IP whitelisting and rate limiting. 

It's built around a .NET MVC API that uses PostgreSQL for securely storing API keys, and Redis for monitoring rate limits. Also included is a Next.js dashboard for interacting with the service using a browser.

The .NET backend uses a custom middleware pipeline to process each request through logging, authentication and rate limiting before reaching the controllers. Once a request has been authenticated, Entity Framework Core handles database access and a response is sent.

**Why I built it**

After finishing Dev Academy Aotearoa I was feeling like diving back into some C#. I had built some functional .NET projects before, but wanted to delve deeper into common tools and concepts like the MVC pattern, dependency injection, middleware and Entity Framework Core.

I had been thinking about deploying some of the APIs I had built in earlier projects, which had me wondering how to protect these from both a security and cost perspective. This is what made me want to focus on authenticating HTTP requests and enforcing rate limits with Redis. 

It was also the perfect opportunity to deploy something slightly more complex on AWS, and get some basic experience with Docker and Terraform. Setting up a CI/CD pipeline with GitHub Actions was also on my list.

## Key features 💡

**.NET Architecture**

- MVC pattern with controllers, services, and models
- Custom middleware pipeline i.e. logging > auth > rate limit > controller
- Service layer with dependency injection (`IApiKeyService`, `IRateLimitService`)
- Entity Framework Core for PostgreSQL data access


**API Key Management**

- Cryptographically secure key generation using `RandomNumberGenerator`
- Keys stored as BCrypt hashes
- Admin ability to generate, validate, revoke and delete keys
- Optional expiration dates and usage tracking

**API Key Authentication**

- Dual authentication approach i.e. `X-Api-Key` for users, `X-Admin-Key` for admin endpoints
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

- I was considering using a combination of RDS (Postgres) and ElastiCache (Redis), but found that for this scale it was cheaper and easier to use EC2

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

Tests require Redis to be running on `localhost:6379`. Start it first, then run the tests:

```bash
# Start Redis (if not already running)
docker compose up redis -d

# Run tests
dotnet test
```

**Local Development (without Docker for .NET API)**

If you prefer to run the .NET API locally with `dotnet run` (useful for debugging), start only the infrastructure containers:

```bash
# Start only PostgreSQL and Redis
docker compose up db redis -d
```

Then create a `.env` file in the root with connection strings for local development:

```
POSTGRES_USER=your-username
POSTGRES_PASSWORD=your-password
POSTGRES_DB=mithrandirdb
ADMIN_API_KEY=your-admin-key
ConnectionStrings__MithrandirDb=Host=localhost;Port=5432;Database=mithrandirdb;Username=your-username;Password=your-password
ConnectionStrings__MithrandirRedis=localhost:6379
CORS_ORIGINS=http://localhost:3000
```

Run the API:

```bash
cd src
dotnet run
```

The API will be available at `http://localhost:5193`. Update your dashboard's `.env.local` to use this URL:

```
NEXT_PUBLIC_DOTNET_API_URL=http://localhost:5193
ADMIN_API_KEY=your-admin-key
```
