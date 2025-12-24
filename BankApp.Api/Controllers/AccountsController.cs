using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Core.Models;
using BankApp.Api.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace BankApp.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")] // Базовый URL: /api/accounts
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public AccountsController(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

         // GET: api/accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountResponseDto>>> GetAccounts()
        {
            var accounts = await _accountService.GetAllAsync();
            // Используем маппер для преобразования в DTO
            var accountDtos = _mapper.Map<IEnumerable<AccountResponseDto>>(accounts);
            return Ok(accountDtos); // 200 OK
        }

        // GET: api/accounts/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountResponseDto>> GetAccount(Guid id)
        {
            var account = await _accountService.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound(); 
            }
            // Используем маппер для преобразования в DTO
            var accountDto = _mapper.Map<AccountResponseDto>(account);
            return Ok(accountDto); // 200 OK
        }

        // POST: api/accounts
        [HttpPost]
        public async Task<ActionResult<AccountResponseDto>> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                var account = await _accountService.CreateAccountAsync(
                    request.AccountType, 
                    request.OwnerName, 
                    request.InitialDeposit
                );
                // Маппим результат перед возвратом
                var accountDto = _mapper.Map<AccountResponseDto>(account);
                return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, accountDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request (Неверный тип счета/сумма)
            }
             catch (InvalidOperationException ex)
            {
                return Conflict(new { error = "Account creation failed.", details = ex.Message }); // 409 Conflict
            }
        }

        // POST: api/accounts/{accountId}/deposit
        [HttpPost("{accountId}/deposit")]
        public async Task<IActionResult> Deposit(Guid accountId, [FromBody] DepositRequest request)
        {
            try
            {
                await _accountService.DepositAsync(accountId, request.Amount);
                return Ok(new { message = "Deposit successful." }); // 200 OK
            }
            catch (KeyNotFoundException)
            {
                return NotFound(); // 404 Not Found (Счет не найден)
            }
            
        }

        // POST: api/accounts/{accountId}/withdraw
        [HttpPost("{accountId}/withdraw")]
        public async Task<IActionResult> Withdraw(Guid accountId, [FromBody] WithdrawRequest request)
        {
            try
            {
                await _accountService.WithdrawAsync(accountId, request.Amount);
                return Ok(new { message = "Withdrawal successful." }); // 200 OK
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InsufficientFundsException ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request (Недостаточно средств)
            }
    
        }

        // POST: api/accounts/{fromAccountId}/transfer
        [HttpPost("{fromAccountId}/transfer")]
        public async Task<IActionResult> Transfer(Guid fromAccountId, [FromBody] TransferRequest request)
        {
            try
            {
                await _accountService.TransferAsync(fromAccountId, request.ToAccountId, request.Amount);
                return Ok(new { message = "Transfer successful." }); // 200 OK
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex) when (ex is InvalidAmountException || ex is InsufficientFundsException)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }
        }

        // GET: api/accounts/{accountId}/statement
        [HttpGet("{accountId}/statement")]
        // Меняем возвращаемый тип с Transaction на TransactionResponseDto
        public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetStatement(Guid accountId)
        {
            try
            {
                var transactions = await _accountService.GetStatementAsync(accountId);
                // Маппим список транзакций в DTO
                var transactionDtos = _mapper.Map<IEnumerable<TransactionResponseDto>>(transactions);
                return Ok(transactionDtos); // 200 OK
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
        
        // POST: api/accounts/monthly-process
        [HttpPost("monthly-process")]
        public async Task<IActionResult> RunMonthlyProcess()
        {
            await _accountService.RunMonthlyProcessAsync();
            return Ok(new { message = "Monthly process initiated." }); // 200 OK
        }
    }
}
