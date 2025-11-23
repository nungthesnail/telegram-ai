FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TelegramAi.Api/TelegramAi.Api.csproj", "src/TelegramAi.Api/"]
COPY ["src/TelegramAi.Application/TelegramAi.Application.csproj", "src/TelegramAi.Application/"]
COPY ["src/TelegramAi.Domain/TelegramAi.Domain.csproj", "src/TelegramAi.Domain/"]
COPY ["src/TelegramAi.Infrastructure/TelegramAi.Infrastructure.csproj", "src/TelegramAi.Infrastructure/"]
COPY ["src/TelegramAi.TelegramBot/TelegramAi.TelegramBot.csproj", "src/TelegramAi.TelegramBot/"]
RUN dotnet restore "src/TelegramAi.Api/TelegramAi.Api.csproj"
COPY . .
WORKDIR "/src/src/TelegramAi.Api"
RUN dotnet build "TelegramAi.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TelegramAi.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramAi.Api.dll"]
