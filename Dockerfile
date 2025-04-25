# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln ./
COPY Laufevent/*.csproj ./Laufevent/

# Restore dependencies
RUN dotnet restore

# Copy the remaining source code
COPY Laufevent/. ./Laufevent/

# Set working directory to the project folder
WORKDIR /app/Laufevent

# Publish the application to /publish folder
RUN dotnet publish -c Release -o /publish/

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
#WORKDIR /publish
# Copy the published output from the build stage
COPY --from=build /publish ./

# Set the environment variable to ensure Kestrel binds to HTTP only
ENV ASPNETCORE_URLS=http://+:8080

# Expose port 80 for HTTP traffic
#EXPOSE 8080

# Set the entry point for the container to run the app
ENTRYPOINT ["dotnet", "Laufevent.dll"]

RUN echo "Running new version"