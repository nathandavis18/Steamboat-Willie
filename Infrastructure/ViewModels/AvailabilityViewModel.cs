namespace Infrastructure.ViewModels
{
    public class AvailabilityModel
    {
        public required string AvailabilityId { get; set; }
        public bool IsAppointment { get; set; }
        public string? ClientName { get; set; }
        public string? WNumber { get; set; }
        public string? ClientEmail { get; set; }
        public required string Date { get; set; }
        public required string Time { get; set; }
        public required string Location { get; set; }
        public string? Comments { get; set; }
        public string? CancelReason { get; set; }
        public bool StudentNoShow { get; set; }
        public DateTime TimeDate { get; set; }
    }
}
