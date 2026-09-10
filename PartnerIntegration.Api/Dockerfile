# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore PartnerIntegration.sln

RUN dotnet publish \
    PartnerIntegration.Api/PartnerIntegration.Api.csproj \
    -c Release \
    -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PartnerIntegration.Api.dll"]