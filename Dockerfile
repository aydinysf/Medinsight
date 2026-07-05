FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MedInsight.sln global.json Directory.Build.props ./
COPY src/MedInsight.Domain/MedInsight.Domain.csproj src/MedInsight.Domain/
COPY src/MedInsight.Application/MedInsight.Application.csproj src/MedInsight.Application/
COPY src/MedInsight.Infrastructure/MedInsight.Infrastructure.csproj src/MedInsight.Infrastructure/
COPY src/MedInsight.Shared/MedInsight.Shared.csproj src/MedInsight.Shared/
COPY src/MedInsight.Dicom/MedInsight.Dicom.csproj src/MedInsight.Dicom/
COPY src/MedInsight.Reporting/MedInsight.Reporting.csproj src/MedInsight.Reporting/
COPY src/MedInsight.Api/MedInsight.Api.csproj src/MedInsight.Api/
RUN dotnet restore MedInsight.sln

COPY src/ src/
RUN dotnet publish src/MedInsight.Api/MedInsight.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MedInsight.Api.dll"]
