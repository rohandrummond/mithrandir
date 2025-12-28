# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy project file and restore dependencies
COPY src/*.csproj ./src/
RUN dotnet restore src/mithrandir.csproj

# Copy source code and publish
COPY src/ ./src/
RUN dotnet publish src/mithrandir.csproj -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published app from build stage
COPY --from=build /app ./

# Switch to non-root user
USER appuser

# Expose port (ASP.NET Core defaults to 8080 in .NET 8+)
EXPOSE 8080

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "mithrandir.dll"]
