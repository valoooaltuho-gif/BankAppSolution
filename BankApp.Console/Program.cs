using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Globalization;
using System.Collections.Generic;

class Program
{
    // Убедись, что порт (5124) совпадает с твоим запущенным API
    private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5124/") };
    private static string? _token;

    static async Task Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Console.WriteLine("=== BankApp Console Client 2025 ===");

        // Шаг 1: Авторизация (обязательно для [Authorize] контроллеров)
        while (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }

        // Шаг 2: Главное меню
        await RunMenuAsync();
    }

    private static async Task AuthenticateAsync()
    {
        Console.WriteLine("\n--- Login ---");
        Console.Write("Username: ");
        string? user = Console.ReadLine();
        Console.Write("Password: ");
        string? pass = Console.ReadLine();

        var loginDto = new { UserName = user, Password = pass };
        var response = await _httpClient.PostAsJsonAsync("api/Auth/login", loginDto);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            _token = result?.Token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            Console.WriteLine("Successfully logged in!");
        }
        else
        {
            Console.WriteLine("Login failed. Check your username and password.");
        }
    }

    private static async Task RunMenuAsync()
    {
        while (true)
        {
            Console.WriteLine("\n--- Banking Operations ---");
            Console.WriteLine("1. List All Accounts");
            Console.WriteLine("2. Create New Account");
            Console.WriteLine("3. Deposit");
            Console.WriteLine("4. Withdraw");
            Console.WriteLine("5. Transfer");
            Console.WriteLine("6. View Statement (Transactions)");
            Console.WriteLine("7. Exit");
            Console.Write("Select action: ");

            switch (Console.ReadLine())
            {
                case "1": await ListAccountsAsync(); break;
                case "2": await CreateAccountAsync(); break;
                case "3": await DepositAsync(); break;
                case "4": await WithdrawAsync(); break;
                case "5": await TransferAsync(); break;
                case "6": await ViewStatementAsync(); break;
                case "7": return;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }

    private static async Task ListAccountsAsync()
    {
        try 
        {
            var accounts = await _httpClient.GetFromJsonAsync<List<AccountResponseDto>>("api/Accounts");
            Console.WriteLine("\n{0,-40} | {1,-15} | {2,-10} | {3}", "Account ID", "Owner", "Balance", "Type");
            Console.WriteLine(new string('-', 85));
            foreach (var acc in accounts ?? new())
            {
                Console.WriteLine("{0,-40} | {1,-15} | {2,-10:N2} | {3}", acc.Id, acc.OwnerName, acc.Balance, acc.AccountType);
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    private static async Task CreateAccountAsync()
    {
        Console.Write("Enter Account Type (Savings/Checking/Business): ");
        string type = Console.ReadLine() ?? "Checking";
        Console.Write("Enter Owner Name: ");
        string owner = Console.ReadLine() ?? "";
        Console.Write("Initial Deposit: ");
        decimal.TryParse(Console.ReadLine(), out decimal deposit);

        var request = new { AccountType = type, OwnerName = owner, InitialDeposit = deposit };
        var response = await _httpClient.PostAsJsonAsync("api/Accounts", request);

        if (response.IsSuccessStatusCode)
            Console.WriteLine("Account successfully created!");
        else
            Console.WriteLine($"Error: {await response.Content.ReadAsStringAsync()}");
    }

    private static async Task DepositAsync()
    {
        Console.Write("Enter Account ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid id)) return;
        Console.Write("Amount: ");
        decimal.TryParse(Console.ReadLine(), out decimal amount);

        var response = await _httpClient.PostAsJsonAsync($"api/Accounts/{id}/deposit", new { Amount = amount });
        await HandleResponse(response);
    }

    private static async Task WithdrawAsync()
    {
        Console.Write("Enter Account ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid id)) return;
        Console.Write("Amount: ");
        decimal.TryParse(Console.ReadLine(), out decimal amount);

        var response = await _httpClient.PostAsJsonAsync($"api/Accounts/{id}/withdraw", new { Amount = amount });
        await HandleResponse(response);
    }

    private static async Task TransferAsync()
    {
        Console.Write("Source Account ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid fromId)) return;
        Console.Write("Target Account ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid toId)) return;
        Console.Write("Amount: ");
        decimal.TryParse(Console.ReadLine(), out decimal amount);

        var response = await _httpClient.PostAsJsonAsync($"api/Accounts/{fromId}/transfer", new { ToAccountId = toId, Amount = amount });
        await HandleResponse(response);
    }

    private static async Task ViewStatementAsync()
    {
        Console.Write("Enter Account ID: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid id)) return;

        var response = await _httpClient.GetAsync($"api/Accounts/{id}/statement");
        if (response.IsSuccessStatusCode)
        {
            var transactions = await response.Content.ReadFromJsonAsync<List<TransactionResponseDto>>();
            Console.WriteLine("\n{0,-20} | {1,-10} | {2,-10} | {3}", "Date", "Type", "Amount", "Description");
            foreach (var t in transactions ?? new())
            {
                Console.WriteLine("{0,-20:g} | {1,-10} | {2,-10:N2} | {3}", t.Date, t.Type, t.Amount, t.Description);
            }
        }
        else Console.WriteLine("Account not found.");
    }

    private static async Task HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
            Console.WriteLine(result?.Message);
        }
        else
        {
            Console.WriteLine($"Action failed: {await response.Content.ReadAsStringAsync()}");
        }
    }
}

// DTO классы для консоли (должны совпадать по именам с JSON из API)
public record LoginResponse(string Token, string Message);
public record MessageResponse(string Message);
public class AccountResponseDto 
{ 
    public Guid Id { get; set; } 
    public string OwnerName { get; set; } = "";
    public decimal Balance { get; set; } 
    public string AccountType { get; set; } = "";
}
public class TransactionResponseDto
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
}
