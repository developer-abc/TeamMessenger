#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TeamMessenger/Server/TeamMessenger.Server.csproj", "TeamMessenger/Server/"]
COPY ["TeamMessenger/Client/TeamMessenger.Client.csproj", "TeamMessenger/Client/"]
COPY ["TeamMessenger/Shared/TeamMessenger.Shared.csproj", "TeamMessenger/Shared/"]
RUN dotnet restore "TeamMessenger/Server/TeamMessenger.Server.csproj"
COPY . .
WORKDIR "/src/TeamMessenger/Server"
RUN dotnet build "TeamMessenger.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TeamMessenger.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT "dotnet TeamMessenger.Server.dll --urls=http://+:$PORT"