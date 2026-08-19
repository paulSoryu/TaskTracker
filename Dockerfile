# ==============================================================================
# Этап 1: Сборка C# кода (SDK образ)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Копируем файлы проектов (.csproj) по отдельности.
# Это позволяет Docker кэшировать слои восстановления пакетов. Если код в Business 
# изменился, но список пакетов тот же — 'dotnet restore' выполнится мгновенно.
COPY ["TaskTracker.Api/TaskTracker.Api.csproj", "TaskTracker.Api/"]
COPY ["TaskTracker.Business/TaskTracker.Business.csproj", "TaskTracker.Business/"]
COPY ["TaskTracker.DataAccess/TaskTracker.DataAccess.csproj", "TaskTracker.DataAccess/"]
COPY ["TaskTracker.Shared/TaskTracker.Shared.csproj", "TaskTracker.Shared/"]

# 2. Восстанавливаем зависимости для главного исполняемого проекта (скачивает NuGet пакеты)
RUN dotnet restore "TaskTracker.Api/TaskTracker.Api.csproj"

# 3. Копируем абсолютно весь исходный код решения в контейнер
COPY . .

# 4. Компилируем и публикуем API проект в режиме Release в папку /app/publish
RUN dotnet publish "TaskTracker.Api/TaskTracker.Api.csproj" -c Release -o /app/publish

# ==============================================================================
# Этап 2: Запуск готового API (Легковесный ASP.NET рантайм)
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# 5. Копируем только результат сборки из предыдущего этапа
COPY --from=build /app/publish .

# 6. Настройка прав для SQLite базы данных и ключей защиты данных (Data Protection)
# Поскольку мы используем Вариант 1 (база в корне папки приложения), выдаем права 
# встроенному пользователю 'app', чтобы приложение могло писать в файлы рантайма не от root.
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys && \
    chown -R app:app /app /home/app/.aspnet/DataProtection-Keys

# 7. Объявляем тома. Теперь папка /app (где создается TaskTracker.db) замаплена на Volume, 
# что гарантирует сохранность базы данных между перезапусками контейнера.
VOLUME ["/app", "/home/app/.aspnet/DataProtection-Keys"]

# Переключаемся на безопасного пользователя без root-прав (стандарт безопасности .NET)
USER app

# Настройка портов по умолчанию для современных образов .NET (8.0 и новее использует порт 8080)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Проверка работоспособности контейнера
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost:8080/health || exit 1

# Точка входа указывает на исполняемый файл вашего API-слоя
ENTRYPOINT ["dotnet", "TaskTracker.Api.dll"]
