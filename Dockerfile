# Get base SDK image from MS
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /src

 # Copy the CSPROJ file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the project files and build a release
COPY . ./
RUN dotnet publish -c Release -o out

# Generate a runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim
WORKDIR /app
EXPOSE 8080
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "LAPS WebUI.dll"] 
