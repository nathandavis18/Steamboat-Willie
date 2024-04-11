namespace Infrastructure.ViewModels
{
    public class AvailabilityModel
    {
        public string AvailabilityId { get; set; }
        public bool IsAppointment { get; set; }
        public string? ClientName { get; set; }
        public string? WNumber { get; set; }
        public string? ClientEmail { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Location { get; set; }
        public string Comments { get; set; }
        public string? CancelReason { get; set; }
    }
}
