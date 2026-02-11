FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Docker
WORKDIR /src
COPY ["MailService/MailService.Api/MailService.Api.csproj", "MailService.Api/"]
COPY ["MailService/MailService.Application/MailService.Application.csproj", "MailService.Application/"]
COPY ["MailService/MailService.Infrastructure/MailService.Infrastructure.csproj", "MailService.Infrastructure/"]
COPY nuget.config .
COPY nuget-packages ./nuget-packages
RUN dotnet restore "MailService.Api/MailService.Api.csproj"
COPY ["MailService/", "MailService/"]
WORKDIR "/src/MailService/MailService.Api"
RUN dotnet build "MailService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Docker
RUN dotnet publish "MailService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MailService.Api.dll"]
