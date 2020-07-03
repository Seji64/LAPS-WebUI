FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app
 
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /repo
COPY . .
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
RUN dotnet publish src -c Release -o /publish/laps-webui

FROM base AS final
WORKDIR /app
COPY --from=build publish/laps-webui .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LAPS WebUI.dll"]
