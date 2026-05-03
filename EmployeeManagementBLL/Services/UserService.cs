using AutoMapper;
using EmployeeManagementBLL.Interfaces;
using EmployeeManagementDAL.Interfaces;
using EmployeeManagementDAL.Models;
using EmployeeManagementModel.Constants;
using EmployeeManagementModel.DTOs;
using EmployeeManagementModel.Responses;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace EmployeeManagementBLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly PasswordHasherService _passwordHasher;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repo, PasswordHasherService passwordHasher, JwtTokenService jwtTokenService, ILogger<UserService> logger, IMapper mapper)
        {
            _repo = repo;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<OperationResult<LoginResponseDTO>> Login(LoginDTO login)
        {
            _logger.LogInformation("Login attempt started for {Username}", login.Username);

            var user = await _repo.GetByUsername(login.Username);

            if (user == null)
            {
                _logger.LogWarning("Invalid login - user not found: {Username}", login.Username);

                return new OperationResult<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var passwordValid = _passwordHasher.VerifyPassword(user, user.Password, login.Password);

            if (!passwordValid)
            {
                _logger.LogWarning("Invalid login - wrong password for {Username}", login.Username);

                return new OperationResult<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }

            var accessToken = _jwtTokenService.GenerateToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            await _repo.UpdateUser(user);

            _logger.LogInformation("Login successful for {Username}", login.Username);

            var loginResponse = _mapper.Map<LoginResponseDTO>(user);
            loginResponse.AccessToken = accessToken;
            loginResponse.RefreshToken = refreshToken;

            return new OperationResult<LoginResponseDTO>
            {
                Success = true,
                Message = "Login successful",
                Data = loginResponse
            };
        }

        public async Task<OperationResult> RegisterUser(RegisterDTO register)
        {
            var allowedRoles = new[] { Roles.Admin, Roles.User };

            if (!allowedRoles.Contains(register.Role, StringComparer.OrdinalIgnoreCase))
            {
                return new OperationResult
                {
                    Success = false,
                    Message = $"Role must be {Roles.Admin} or {Roles.User}"
                };
            }

            if (await _repo.IsUsernameExists(register.Username))
            {
                _logger.LogWarning("Duplicate username registration attempted: {Username}", register.Username);
                return new OperationResult
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            if (await _repo.IsEmailExists(register.Email))
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            var newUser = _mapper.Map<AppUser>(register);

            newUser.Username = register.Username.Trim().ToLower();
            newUser.Email = register.Email.Trim().ToLower();
            newUser.Role = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(register.Role.Trim().ToLower());

            newUser.Password = _passwordHasher.HashPassword(newUser, register.Password);

            _logger.LogInformation("User registration started for {Username}", register.Username);
            return await _repo.RegisterUser(newUser);
        }

        public async Task<OperationResult<LoginResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            _logger.LogInformation("Refresh token request started for {Username}", request.Username);

            var user = await _repo.GetByUsername(request.Username);

            if (user == null)
            {
                return new OperationResult<LoginResponseDTO>
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            if (user.RefreshToken != request.RefreshToken)
            {
                return new OperationResult<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Invalid refresh token"
                };
            }

            if (user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return new OperationResult<LoginResponseDTO>
                {
                    Success = false,
                    Message = "Refresh token expired"
                };
            }

            var newAccessToken = _jwtTokenService.GenerateToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            await _repo.UpdateUser(user);

            _logger.LogInformation("Refresh token successful for {Username}", request.Username);

            var refreshResponse = _mapper.Map<LoginResponseDTO>(user);
            refreshResponse.AccessToken = newAccessToken;
            refreshResponse.RefreshToken = newRefreshToken;

            return new OperationResult<LoginResponseDTO>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = refreshResponse
            };
        }
    }
}
