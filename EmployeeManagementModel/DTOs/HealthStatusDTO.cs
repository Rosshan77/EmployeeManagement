namespace EmployeeManagementModel.DTOs
{
    public class HealthStatusDTO
    {
        public string ApiStatus { get; set; } = string.Empty;
        public string DatabaseStatus { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
        public string Environment { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}