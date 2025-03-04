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

# Generate self-signed certificate inside the container
RUN apt-get update && apt-get install -y openssl && \
    mkdir -p /https && \
    openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes \
    -keyout /https/aspnetapp.key -out /https/aspnetapp.crt \
    -subj "/CN=localhost" && \
    openssl pkcs12 -export -out /https/aspnetapp.pfx -inkey /https/aspnetapp.key -in /https/aspnetapp.crt -passout pass:YourPassword

# Copy app files
COPY --from=build /publish .

# Expose HTTP (80) and HTTPS (443)
EXPOSE 80
EXPOSE 443

# Set environment variables for HTTPS
ENV ASPNETCORE_URLS="https://+:443;http://+:80"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/aspnetapp.pfx"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="YourPassword"

# Run the app
ENTRYPOINT ["dotnet", "Laufevent.dll"]
