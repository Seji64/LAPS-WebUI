FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine AS base
WORKDIR /app
 
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /repo
COPY . .
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
RUN dotnet publish src -c Release -o /publish/laps-webui

FROM base AS final
WORKDIR /app
RUN apk update
RUN apk add --no-cache openldap-clients libldap cyrus-sasl openssl curl
RUN rm -rf /var/cache/apk/*

COPY --from=build publish/laps-webui .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LAPS WebUI.dll"]
