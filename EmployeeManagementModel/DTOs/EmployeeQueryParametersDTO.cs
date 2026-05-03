namespace EmployeeManagementModel.DTOs
{
    public class EmployeeQueryParametersDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string? Search { get; set; }
        public string? Department { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc";
    }
}