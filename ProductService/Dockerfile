FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Docker
WORKDIR /src
COPY ["ProductService/ProductService.Api/ProductService.Api.csproj", "ProductService.Api/"]
COPY ["ProductService/ProductService.Domain/ProductService.Domain.csproj", "ProductService.Domain/"]
COPY ["ProductService/ProductService.Application/ProductService.Application.csproj", "ProductService.Application/"]
COPY ["ProductService/ProductService.DataAccess/ProductService.DataAccess.csproj", "ProductService.DataAccess/"]
COPY ["ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "ProductService.Infrastructure/"]
COPY nuget.config .
COPY nuget-packages ./nuget-packages
RUN dotnet restore "ProductService.Api/ProductService.Api.csproj"
COPY ["ProductService/", "ProductService/"]
WORKDIR "/src/ProductService/ProductService.Api"
RUN dotnet build "ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Docker
RUN dotnet publish "ProductService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
