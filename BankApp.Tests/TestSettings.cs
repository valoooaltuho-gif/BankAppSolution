using Microsoft.AspNetCore.Mvc.Testing;

// Указываем путь к файлу проекта API относительно папки BankApp.Tests
[assembly: WebApplicationFactoryContentRoot("BankApp.Api", "..\\BankApp.Api", "BankApp.Api.csproj", "1")]
