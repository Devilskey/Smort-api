#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Api/Smort-api.csproj", "Api/"]
COPY ["Smort-api.Extensions/Smort-api.Extensions.csproj", "Smort-api.Extensions/"]
COPY ["Smort-api.Handlers/Smort-api.Handlers.csproj", "Smort-api.Handlers/"]
COPY ["Smort-api.Object/Smort-api.Objects.csproj", "Smort-api.Object/"]
RUN dotnet restore "Api/Smort-api.csproj"
COPY . .
WORKDIR "/src/Api"
RUN apt-get update && apt-get install -y ffmpeg

RUN dotnet build "Smort-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Smort-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Smort-api.dll"]