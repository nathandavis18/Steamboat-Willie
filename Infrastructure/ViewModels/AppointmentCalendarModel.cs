namespace Infrastructure.ViewModels
{
    public class AppointmentCalendarModel
    {
        public string? Id { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderType { get; set; }
        public string? AppointmentType { get; set; }
        public string? Date { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Location { get; set; }
        public string? Color { get; set; }
        public string? Type { get; set; } //appointment or availability, for provider calendar
        public string? Campus {  get; set; }
    }
}
