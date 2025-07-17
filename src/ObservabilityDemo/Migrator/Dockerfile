FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:22bcafd1f3c3bec34af587d205c631380352b15eeb52f9376f7cb3f6489c1f6f AS base
WORKDIR /app

RUN addgroup --system appgroup \
 && adduser  --system --ingroup appgroup appuser

USER appuser

FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:90872f8e7f1fd2b93989b81fb7f152c3bef4fe817470a3227abaa18c873dba60 AS build
WORKDIR /src

COPY ./Migrator/Migrator.csproj ./Migrator/
COPY ./Shared/Settings/Settings.csproj ./Shared/Settings/

RUN dotnet restore ./Migrator/Migrator.csproj

COPY . .

RUN dotnet build ./Migrator/Migrator.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ./Migrator/Migrator.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Migrator.dll"]
