FROM mcr.microsoft.com/dotnet/core/runtime:3.1-bionic AS base
WORKDIR /app
 
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build
WORKDIR /repo
COPY . .
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
RUN dotnet publish src -c Release -o /publish/laps-webui

FROM base AS final
WORKDIR /app
RUN apt-get -y update
RUN apt-get -y upgrade
RUN apt-get install -y ldap-utils sasl2-bin libsasl2-2 libsasl2-modules libsasl2-modules-ldap openssl
COPY --from=build publish/laps-webui .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LAPS WebUI.dll"]
