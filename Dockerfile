#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NET7.Api/NET7.Api.csproj", "NET7.Api/"]
COPY ["NET7.Infrastructure/NET7.Infrastructure.csproj", "NET7.Infrastructure/"]
COPY ["NET7.Domain/NET7.Domain.csproj", "NET7.Domain/"]
RUN dotnet restore "NET7.Api/NET7.Api.csproj"
COPY . .
WORKDIR "/src/NET7.Api"
RUN dotnet build "NET7.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NET7.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NET7.Api.dll"]