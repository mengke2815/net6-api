#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NET6.Api/NET6.Api.csproj", "NET6.Api/"]
COPY ["NET6.Infrastructure/NET6.Infrastructure.csproj", "NET6.Infrastructure/"]
COPY ["NET6.Domain/NET6.Domain.csproj", "NET6.Domain/"]
RUN dotnet restore "NET6.Api/NET6.Api.csproj"
COPY . .
WORKDIR "/src/NET6.Api"
RUN dotnet build "NET6.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NET6.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NET6.Api.dll"]