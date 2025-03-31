﻿# Use .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and build
COPY *.sln ./
COPY Laufevent/*.csproj ./Laufevent/
RUN dotnet restore

COPY Laufevent/. ./Laufevent/
WORKDIR /app/Laufevent
RUN dotnet publish -c Release -o /publish

# Use ASP.NET runtime for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published app from build stage
COPY --from=build /publish .

# Copy the HTTPS certificate
COPY https/aspnetapp.pfx /https/aspnetapp.pfx

# Ensure correct permissions for the certificate
RUN chmod 644 /https/aspnetapp.pfx

# Expose HTTP (80) and HTTPS (443)
EXPOSE 80
EXPOSE 443

# Run the app
ENTRYPOINT ["dotnet", "Laufevent.dll"]
