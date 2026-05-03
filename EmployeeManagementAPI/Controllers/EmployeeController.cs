using Asp.Versioning;
using EmployeeManagementBLL.Interfaces;
using EmployeeManagementModel.Constants;
using EmployeeManagementModel.DTOs;
using EmployeeManagementModel.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmployeeManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("EmployeePolicy")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService service, ILogger<EmployeeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ================= GET ALL =================
        [HttpGet]
        [Authorize(Roles = Roles.Admin + "," + Roles.User)]
        public async Task<IActionResult> GetAll([FromQuery] EmployeeQueryParametersDTO query)
        {
            _logger.LogInformation("GetAll Employees endpoint called with pagination/filter");

            var employees = await _service.GetAll(query);

            return Ok(new ApiResponse<PagedResultDTO<EmployeeDTO>>
            {
                Success = true,
                Message = "Employees fetched successfully",
                Data = employees
            });
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.User)]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _service.GetById(id);

            if (employee == null)
            {
                return NotFound(new ApiResponse<EmployeeDTO>
                {
                    Success = false,
                    Message = "Employee not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<EmployeeDTO>
            {
                Success = true,
                Message = "Employee fetched successfully",
                Data = employee
            });
        }

        // ================= GET BY EMAIL =================
        [HttpGet("by-email")]
        [Authorize(Roles = Roles.Admin + "," + Roles.User)]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            var employee = await _service.GetByEmail(email);

            if (employee == null)
            {
                _logger.LogWarning("No employees found in database");
                return NotFound(new ApiResponse<EmployeeDTO>
                {
                    Success = false,
                    Message = "Employee not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<EmployeeDTO>
            {
                Success = true,
                Message = "Employee fetched successfully",
                Data = employee
            });
        }

        // ================= ADD =================
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Add([FromBody] EmployeeDTO emp)
        {
            _logger.LogInformation("Adding employee with email {Email}", emp.Email);
            var result = await _service.Add(emp);

            if (!result)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Email already exists",
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Employee added successfully",
                Data = null
            });
        }

        // ================= UPDATE BY EMAIL =================
        [HttpPut("update-by-email")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> UpdateByEmail([FromBody] EmployeeDTO emp)
        {
            var result = await _service.UpdateByEmail(emp);

            if (!result)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Employee not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Employee updated successfully",
                Data = null
            });
        }

        // ================= DELETE BY EMAIL =================
        [HttpDelete("delete-by-email")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteByEmail([FromQuery] string email)
        {
            var result = await _service.DeleteByEmail(email);

            if (!result)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Employee not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Employee deleted successfully",
                Data = null
            });
        }
    }
}
