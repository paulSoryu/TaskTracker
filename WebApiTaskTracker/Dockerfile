# Этап 1: Сборка C# кода
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WebApiTaskTracker.csproj", "./"]
RUN dotnet restore "WebApiTaskTracker.csproj"
COPY . .
RUN dotnet publish "WebApiTaskTracker.csproj" -c Release -o /app/publish

# Этап 2: Запуск готового API
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Создаем структуру папок для базы данных, чтобы путь совпал с appsettings.json
# и выдаём права пользователю app (встроен в образ aspnet), чтобы приложение
# могло писать в эту папку не от root
RUN mkdir -p /app/3_dataaccess/databases /home/app/.aspnet/DataProtection-Keys && \
    chown -R app:app /app/3_dataaccess/databases /home/app/.aspnet/DataProtection-Keys

# Постоянное хранение данных БД между пересозданиями контейнера
VOLUME ["/app/3_DataAccess/Databases", "/home/app/.aspnet/DataProtection-Keys"]

USER app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WebApiTaskTracker.dll"]