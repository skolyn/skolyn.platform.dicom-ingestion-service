# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy and restore dependencies
COPY *.sln .
COPY src/Skolyn.Platform.DicomIngestion.Api/*.csproj src/Skolyn.Platform.DicomIngestion.Api/
COPY src/Skolyn.Platform.DicomIngestion.Application/*.csproj src/Skolyn.Platform.DicomIngestion.Application/
COPY src/Skolyn.Platform.DicomIngestion.Infrastructure/*.csproj src/Skolyn.Platform.DicomIngestion.Infrastructure/
RUN dotnet restore

# Copy the rest of the source code
COPY . .
WORKDIR /app/src/Skolyn.Platform.DicomIngestion.Api
RUN dotnet publish -c Release -o /app/build

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/build ./

# Expose the port and define the entry point
EXPOSE 8080
ENTRYPOINT ["dotnet", "Skolyn.Platform.DicomIngestion.Api.dll"]
