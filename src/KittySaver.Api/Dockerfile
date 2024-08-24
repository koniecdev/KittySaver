FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["KittySaver/KittySaver.csproj", "KittySaver/"]
RUN dotnet restore "KittySaver/KittySaver.csproj"
COPY . .
WORKDIR "/src/KittySaver"
RUN dotnet build "KittySaver.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "KittySaver.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["cert.pfx", "/https/cert/pfx"]
ENTRYPOINT ["dotnet", "KittySaver.dll"]
