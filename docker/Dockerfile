FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY src/OrderFlow.Domain/*.csproj src/OrderFlow.Domain/
COPY src/OrderFlow.Application/*.csproj src/OrderFlow.Application/
COPY src/OrderFlow.Infrastructure/*.csproj src/OrderFlow.Infrastructure/
COPY src/OrderFlow.API/*.csproj src/OrderFlow.API/
RUN dotnet restore

COPY src/ src/
WORKDIR /src/src/OrderFlow.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "OrderFlow.API.dll"]
