# Build context is the repository root (the API references the other projects).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for restore-layer caching.
COPY ["src/GYMPlanner.Domain/GYMPlanner.Domain.csproj", "src/GYMPlanner.Domain/"]
COPY ["src/GYMPlanner.Application/GYMPlanner.Application.csproj", "src/GYMPlanner.Application/"]
COPY ["src/GYMPlanner.Infrastructure/GYMPlanner.Infrastructure.csproj", "src/GYMPlanner.Infrastructure/"]
COPY ["src/GYMPlanner.Api/GYMPlanner.Api.csproj", "src/GYMPlanner.Api/"]
RUN dotnet restore "src/GYMPlanner.Api/GYMPlanner.Api.csproj"

# Copy the rest and publish.
COPY . .
RUN dotnet publish "src/GYMPlanner.Api/GYMPlanner.Api.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GYMPlanner.Api.dll"]
