# Этап 1: Сборка приложения
# Используем официальный образ SDK .NET 10.0 для сборки
FROM ://mcr.microsoft.com AS build
WORKDIR /app

# Копируем файлы проекта и восстанавливаем зависимости
COPY *.slnx .
COPY BankApp.Api/*.csproj ./BankApp.Api/
COPY BankApp.Console/*.csproj ./BankApp.Console/
COPY BankApp.Core/*.csproj ./BankApp.Core/
COPY BankApp.Infrastructure/*.csproj ./BankApp.Infrastructure/
COPY BankApp.Services/*.csproj ./BankApp.Services/
COPY BankApp.Tests/*.csproj ./BankApp.Tests/

RUN dotnet restore BankApp.slnx

# Копируем остальной код
COPY . .

# Публикуем API проект в папку 'out/api'
RUN dotnet publish BankApp.Api/BankApp.Api.csproj -c Release -o /app/out/api --no-restore

# Этап 2: Финальный образ (runtime only)
# Используем минимальный образ ASP.NET Runtime для запуска
FROM ://mcr.microsoft.com AS final
WORKDIR /app
COPY --from=build /app/out/api .

# Указываем порт, который будет слушать приложение (стандартный для ASP.NET)
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "BankApp.Api.dll"]
