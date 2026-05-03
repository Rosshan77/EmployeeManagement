using AutoMapper;
using EmployeeManagementBLL.Interfaces;
using EmployeeManagementDAL.Interfaces;
using EmployeeManagementDAL.Models;
using EmployeeManagementModel.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace EmployeeManagementBLL.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IConnectionMultiplexer _redis;

        private const string EmployeeCacheRegistryKey = "EmployeeManagementAPI_employee_cachekeys";

        public EmployeeService(
            IEmployeeRepository repo,
            IMapper mapper,
            IDistributedCache cache,
            ILogger<EmployeeService> logger,
            IConnectionMultiplexer redis)
        {
            _repo = repo;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _redis = redis;
        }

        public async Task<PagedResultDTO<EmployeeDTO>> GetAll(EmployeeQueryParametersDTO query)
        {
            var search = query.Search?.Trim().ToLower() ?? "na";
            var department = query.Department?.Trim().ToLower() ?? "na";
            var sortBy = query.SortBy?.Trim().ToLower() ?? "na";
            var sortOrder = query.SortOrder?.Trim().ToLower() ?? "na";

            string cacheKey = $"employees_{query.PageNumber}_{query.PageSize}_{search}_{department}_{sortBy}_{sortOrder}";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("REDIS CACHE HIT : {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<PagedResultDTO<EmployeeDTO>>(cachedData)!;
            }

            _logger.LogInformation("REDIS CACHE MISS : {CacheKey} -> Fetching from database", cacheKey);

            var result = await _repo.GetAll(query);

            var mappedResult = new PagedResultDTO<EmployeeDTO>
            {
                CurrentPage = result.CurrentPage,
                PageSize = result.PageSize,
                TotalRecords = result.TotalRecords,
                TotalPages = result.TotalPages,
                Data = _mapper.Map<List<EmployeeDTO>>(result.Data)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(mappedResult),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

            await RegisterCacheKey(cacheKey);

            return mappedResult;
        }

        public async Task<EmployeeDTO?> GetById(int id)
        {
            string cacheKey = $"employee_id_{id}";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("REDIS CACHE HIT : {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<EmployeeDTO>(cachedData);
            }

            _logger.LogInformation("REDIS CACHE MISS : {CacheKey} -> Fetching from database", cacheKey);

            var employee = await _repo.GetById(id);

            if (employee == null)
                return null;

            var mappedEmployee = _mapper.Map<EmployeeDTO>(employee);

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(mappedEmployee),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

            await RegisterCacheKey(cacheKey);

            return mappedEmployee;
        }

        public async Task<EmployeeDTO?> GetByEmail(string email)
        {
            string normalizedEmail = email.Trim().ToLower();
            string cacheKey = $"employee_email_{normalizedEmail}";

            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("REDIS CACHE HIT : {CacheKey}", cacheKey);
                return JsonSerializer.Deserialize<EmployeeDTO>(cachedData);
            }

            _logger.LogInformation("REDIS CACHE MISS : {CacheKey} -> Fetching from database", cacheKey);

            var employee = await _repo.GetByEmail(email);

            if (employee == null)
                return null;

            var mappedEmployee = _mapper.Map<EmployeeDTO>(employee);

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(mappedEmployee),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });

            await RegisterCacheKey(cacheKey);

            return mappedEmployee;
        }

        public async Task<bool> Add(EmployeeDTO emp)
        {
            var exists = await _repo.GetByEmail(emp.Email);

            if (exists != null)
                return false;

            var employee = _mapper.Map<Employee>(emp);

            await _repo.Add(employee);

            await ClearEmployeeCache();

            return true;
        }

        public async Task<bool> UpdateByEmail(EmployeeDTO emp)
        {
            var employee = _mapper.Map<Employee>(emp);

            var result = await _repo.UpdateByEmail(employee);

            if (result)
                await ClearEmployeeCache();

            return result;
        }

        public async Task<bool> DeleteByEmail(string email)
        {
            var result = await _repo.DeleteByEmail(email);

            if (result)
                await ClearEmployeeCache();

            return result;
        }

        private async Task RegisterCacheKey(string cacheKey)
        {
            var db = _redis.GetDatabase();
            await db.SetAddAsync(EmployeeCacheRegistryKey, cacheKey);
        }

        private async Task ClearEmployeeCache()
        {
            var db = _redis.GetDatabase();

            var keys = await db.SetMembersAsync(EmployeeCacheRegistryKey);

            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key.ToString());
            }

            await db.KeyDeleteAsync(EmployeeCacheRegistryKey);

            _logger.LogInformation("REDIS CACHE CLEARED : All employee related distributed cache removed");
        }
    }
}