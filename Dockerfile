FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
RUN ls -lah /home/runner/work/
WORKDIR /home/runner/work/src
RUN ls -lah
RUN dotnet restore "LAPS WebUI.csproj"
RUN dotnet build "LAPS WebUI.csproj" -c Release -o /app/build
RUN ls -lah /home/runner/work/
RUN ls -lah /app/build
FROM build AS publish
RUN dotnet publish "LAPS WebUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LAPS WebUI.dll"]
