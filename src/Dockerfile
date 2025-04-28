#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LAPS-WebUI.csproj", "."]
RUN dotnet restore "./LAPS-WebUI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "LAPS-WebUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LAPS-WebUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN apt update && \
    apt upgrade -y && \
    apt install --no-install-recommends -y ca-certificates libldap-common gcc python3 python3-dev python3-pip libkrb5-dev curl && \
    apt clean && \
    rm -rf /var/lib/apt/lists/* && \
    ln -s /usr/lib/x86_64-linux-gnu/libldap-2.5.so.0 /usr/lib/libldap.so.2 && \
    ln -s /usr/lib/x86_64-linux-gnu/liblber-2.5.so.0 /usr/lib/liblber.so.2 && \
    pip3 install dpapi-ng[kerberos] --break-system-packages
COPY --from=publish /app/publish .
HEALTHCHECK CMD curl --fail http://localhost:8080/healthz || exit
USER app
ENTRYPOINT ["dotnet", "LAPS-WebUI.dll"]