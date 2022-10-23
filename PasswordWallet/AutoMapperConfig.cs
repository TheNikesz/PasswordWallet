using AutoMapper;
using PasswordWallet.DTOs.SavedPassword;
using PasswordWallet.Entities;

namespace PasswordWallet;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<SavedPassword, SavedPasswordDto>();
    }
}