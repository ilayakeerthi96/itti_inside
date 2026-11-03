# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY global.json NuGet.config Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet publish src/Itti.Inside.Web/Itti.Inside.Web.csproj --configuration Release --output /app

# Run as the image's non-root user
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Itti.Inside.Web.dll"]
