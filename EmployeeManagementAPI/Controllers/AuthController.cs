using Asp.Versioning;
using EmployeeManagementBLL.Interfaces;
using EmployeeManagementModel.DTOs;
using EmployeeManagementModel.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmployeeManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [EnableRateLimiting("AuthPolicy")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            _logger.LogInformation("Register endpoint called for {Username}", register.Username);

            var result = await _userService.RegisterUser(register);

            var response = new ApiResponse<object>
            {
                Success = result.Success,
                Message = result.Message,
                Data = null
            };

            if (!result.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            _logger.LogInformation("Login endpoint called for {Username}", login.Username);

            var result = await _userService.Login(login);

            var response = new ApiResponse<LoginResponseDTO>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data
            };

            if (!result.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDTO request)
        {
            _logger.LogInformation("RefreshToken endpoint called for {Username}", request.Username);

            var result = await _userService.RefreshToken(request);

            var response = new ApiResponse<LoginResponseDTO>
            {
                Success = result.Success,
                Message = result.Message,
                Data = result.Data
            };

            if (!result.Success)
                return Unauthorized(response);

            return Ok(response);
        }
    }
}