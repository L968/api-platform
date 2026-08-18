FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG PROJECT
COPY backend/Directory.Build.props backend/
COPY backend/src/${PROJECT}/${PROJECT}.csproj backend/src/${PROJECT}/
RUN dotnet restore backend/src/${PROJECT}/${PROJECT}.csproj

COPY backend backend
RUN dotnet publish backend/src/${PROJECT}/${PROJECT}.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ARG PROJECT
ENV APP_DLL=${PROJECT}.dll \
    ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
COPY database/seed.sql /app/database/seed.sql

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "exec dotnet \"$APP_DLL\""]
