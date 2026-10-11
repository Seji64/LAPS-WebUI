#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
# Build tools are only needed to compile the Kerberos bindings; they are removed again in the same layer
RUN apt update && \
    apt upgrade -y && \
    apt install --no-install-recommends -y ca-certificates libldap-common python3 python3-pip libkrb5-3 libgssapi-krb5-2 curl && \
    apt install --no-install-recommends -y gcc python3-dev libkrb5-dev && \
    pip3 install --no-cache-dir "dpapi-ng[kerberos]==0.2.0" --break-system-packages && \
    apt purge -y --auto-remove gcc python3-dev libkrb5-dev && \
    python3 -c "import dpapi_ng, gssapi, krb5" && \
    apt clean && \
    rm -rf /var/lib/apt/lists/* && \
    ln -s /usr/lib/x86_64-linux-gnu/libldap-2.5.so.0 /usr/lib/libldap.so.2 && \
    ln -s /usr/lib/x86_64-linux-gnu/liblber-2.5.so.0 /usr/lib/liblber.so.2
COPY --from=publish /app/publish .
RUN chown app -R /app/
HEALTHCHECK CMD curl --fail http://localhost:8080/healthz || exit
USER app
ENTRYPOINT ["dotnet", "LAPS-WebUI.dll"]