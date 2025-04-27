FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app


COPY *.sln ./
COPY Laufevent/*.csproj ./Laufevent/


RUN dotnet restore

COPY Laufevent/. ./Laufevent/


WORKDIR /app/Laufevent


RUN dotnet publish -c Release -o /publish/


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /publish ./


ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080
ENTRYPOINT ["dotnet", "Laufevent.dll"]

RUN echo "Running new version"