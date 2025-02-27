# Use the official .NET SDK image for building the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# Use the runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Expose the correct port
EXPOSE 44320

# Set the entrypoint for the application
CMD ["dotnet", "Laufevent.dll"]
