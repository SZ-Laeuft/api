﻿FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln ./
COPY Laufevent/*.csproj ./Laufevent/
RUN dotnet restore

COPY Laufevent/. ./Laufevent/
WORKDIR /app/Laufevent
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /publish .
RUN mkdir /https
COPY /https/aspnetapp.pfx /https/aspnetapp.pfx
RUN chmod 644 /https/aspnetapp.pfx

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Laufevent.dll"]
