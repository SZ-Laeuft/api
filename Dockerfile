# Use .NET SDK to build the app
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
COPY --from=build /publish .

# Expose HTTP (80) and HTTPS (443)
EXPOSE 80
EXPOSE 443

# Set environment variables for HTTPS
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_HTTPS_PORT=443

# 🔹 OPTIONAL: Only copy the certificate if it exists (prevents build failure)
COPY --chown=appuser:appgroup https/aspnetapp.pfx /https/aspnetapp.pfx 2>/dev/null || echo "Certificate not found, skipping."

# Run the app
ENTRYPOINT ["dotnet", "Laufevent.dll"]
