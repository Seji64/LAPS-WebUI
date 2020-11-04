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
RUN apk add --no-cache openldap-clients openssl curl
RUN rm -rf /var/cache/apk/*

RUN curl -LJO http://pki.services.k-sys.io/cert/Primary_CA.crt
RUN openssl x509 -inform DER -in Primary_CA.crt -out Primary_CA.pem -text
RUN cp Primary_CA.pem /usr/local/share/ca-certificates/Primary_CA.pem
RUN update-ca-certificates
RUN rm -f Primary_CA.crt Primary_CA.pem

COPY --from=build publish/laps-webui .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LAPS WebUI.dll"]
