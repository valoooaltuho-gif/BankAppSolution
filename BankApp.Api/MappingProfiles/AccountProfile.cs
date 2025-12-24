using AutoMapper;
using BankApp.Core.Models;
using BankApp.Api.DTOs;
using System.Linq;

namespace BankApp.Api.MappingProfiles
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            // Маппинг из доменной модели Transaction в TransactionResponseDto
            CreateMap<Transaction, TransactionResponseDto>();

            // Маппинг из доменной модели Account в AccountResponseDto
            CreateMap<Account, AccountResponseDto>()
                .ForMember(
                    dest => dest.AccountType, 
                    opt => opt.MapFrom(src => src.GetType().Name.Replace("Account", ""))) // Вычисляем тип аккаунта
                .ForMember(
                    dest => dest.Transactions,
                    opt => opt.MapFrom(src => src.Transactions) // Вложенный маппинг
                );
        }
    }
}
