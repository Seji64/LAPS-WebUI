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
RUN apt-get install -y ldap-utils slapd sasl2-bin ca-certificates libsasl2-2 curl libsasl2-modules libsasl2-modules-db libsasl2-modules-gssapi-mit libsasl2-modules-ldap libsasl2-modules-otp libsasl2-modules-sql openssl lapd krb5-kdc-ldap
COPY --from=build publish/laps-webui .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LAPS WebUI.dll"]
