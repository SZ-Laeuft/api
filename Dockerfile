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

# Copy published app from build stage
COPY --from=build /publish .

# Create directory for HTTPS certificate (optional, volume will handle it)
RUN mkdir -p /https

# Expose HTTP (80) and HTTPS (443)
EXPOSE 80
EXPOSE 443

# Set environment variables for HTTPS (path must match volume mount)
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="htl"

# Run the app
ENTRYPOINT ["dotnet", "Laufevent.dll"]
