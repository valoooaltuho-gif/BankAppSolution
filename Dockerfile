# Этап 1: Сборка и Тесты
FROM ://mcr.microsoft.com AS build
WORKDIR /app

# Копируем решение и проекты для кэширования слоев
COPY *.slnx . 
COPY BankApp.Api/*.csproj ./BankApp.Api/
COPY BankApp.Console/*.csproj ./BankApp.Console/
COPY BankApp.Core/*.csproj ./BankApp.Core/
COPY BankApp.Infrastructure/*.csproj ./BankApp.Infrastructure/
COPY BankApp.Services/*.csproj ./BankApp.Services/
COPY BankApp.Tests/*.csproj ./BankApp.Tests/

RUN dotnet restore BankApp.slnx

# Копируем весь исходный код
COPY . .

# Запускаем тесты прямо при сборке образа (необязательно, но надежно)
# Если тесты упадут, сборка образа прервется
RUN dotnet test --no-restore -c Release

# Публикуем
RUN dotnet publish BankApp.Api/BankApp.Api.csproj -c Release -o /app/out/api --no-restore

# Этап 2: Финальный образ
FROM ://mcr.microsoft.com AS final
WORKDIR /app
COPY --from=build /app/out/api .

# В .NET 10 порт 8080 используется по умолчанию
EXPOSE 8080
ENTRYPOINT ["dotnet", "BankApp.Api.dll"]
