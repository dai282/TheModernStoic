# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the Solution and Project files first (for better caching)
COPY ["TheModernStoic.slnx", "./"]
COPY ["TheModernStoic.API/TheModernStoic.API.csproj", "TheModernStoic.API/"]
COPY ["TheModernStoic.Application/TheModernStoic.Application.csproj", "TheModernStoic.Application/"]
COPY ["TheModernStoic.Domain/TheModernStoic.Domain.csproj", "TheModernStoic.Domain/"]
COPY ["TheModernStoic.Infrastructure/TheModernStoic.Infrastructure.csproj", "TheModernStoic.Infrastructure/"]
COPY ["TheModernStoic.Tests/TheModernStoic.Tests.csproj", "TheModernStoic.Tests/"]

# Restore dependencies
#RUN dotnet restore "TheModernStoic.slnx"
RUN dotnet restore "TheModernStoic.API/TheModernStoic.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and Publish
WORKDIR "/src/TheModernStoic.API"
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TheModernStoic.API.dll"]