using MaxemusAPI.Models.Dtos;
using MaxemusAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MaxemusAPI.Models.Helper;

namespace MaxemusAPI.Repository.IRepository
{
    public interface IAccountRepository
    {
        bool IsUniqueUser(string email, string phoneNumber);
        Task<ApplicationUser> IsValidUser(string EmailOrPhone);
        bool IsUniqueEmail(string email);
        bool IsUniquePhone(string phoneNumber);
        Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<LoginResponseDTO> Register(RegisterationRequestDTO registerationRequestDTO);

    }
}
