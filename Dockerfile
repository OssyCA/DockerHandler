FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY Directory.Build.props .
COPY src/DockerController.Api/DockerController.Api.csproj src/DockerController.Api/
COPY src/DockerController.Core/DockerController.Core.csproj src/DockerController.Core/
COPY src/DockerController.Docker/DockerController.Docker.csproj src/DockerController.Docker/
RUN dotnet restore src/DockerController.Api/DockerController.Api.csproj

COPY src/ src/
RUN dotnet publish src/DockerController.Api/DockerController.Api.csproj \
    --configuration $BUILD_CONFIGURATION \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "DockerController.Api.dll"]
