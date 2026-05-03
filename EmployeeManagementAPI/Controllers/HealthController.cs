using Asp.Versioning;
using EmployeeManagementDAL.Context;
using EmployeeManagementModel.DTOs;
using EmployeeManagementModel.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HealthController> _logger;
        private readonly IWebHostEnvironment _environment;

        public HealthController(AppDbContext context, ILogger<HealthController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> CheckHealth()
        {
            _logger.LogInformation("Health check endpoint called");

            bool dbConnected = await _context.Database.CanConnectAsync();

            var data = new HealthStatusDTO
            {
                ApiStatus = "Running",
                DatabaseStatus = dbConnected ? "Connected" : "Disconnected",
                ServerTime = DateTime.Now,
                Environment = _environment.EnvironmentName,
                MachineName = Environment.MachineName,
                Version = "1.0.0"
            };

            if (!dbConnected)
            {
                return StatusCode(500, new ApiResponse<HealthStatusDTO>
                {
                    Success = false,
                    Message = "Application unhealthy",
                    Data = data
                });
            }

            return Ok(new ApiResponse<HealthStatusDTO>
            {
                Success = true,
                Message = "Application is healthy",
                Data = data
            });
        }
    }
}