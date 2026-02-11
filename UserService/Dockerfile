FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Docker
WORKDIR /src
COPY ["UserService/UserService.Api/UserService.Api.csproj", "UserService.Api/"]
COPY ["UserService/UserService.Infrastructure/UserService.Infrastructure.csproj", "UserService.Infrastructure/"]
COPY ["UserService/UserService.Application/UserService.Application.csproj", "UserService.Application/"]
COPY ["UserService/UserService.Domain/UserService.Domain.csproj", "UserService.Domain/"]
COPY ["UserService/UserService.DataAccess/UserService.DataAccess.csproj", "UserService.DataAccess/"]
COPY nuget.config .
COPY nuget-packages ./nuget-packages
RUN dotnet restore "UserService.Api/UserService.Api.csproj"
COPY ["UserService/", "UserService/"]
WORKDIR "/src/UserService/UserService.Api"
RUN dotnet build "UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Docker
RUN dotnet publish "UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
