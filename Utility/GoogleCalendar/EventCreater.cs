using Google.Apis.Calendar.v3.Data;

namespace Utility.GoogleCalendar
{
    public class EventCreater
    {
        public static Event CreateEvent(string summary, string location, string description, string start, string end)
        {
            Event gEvent = new Event()
            {
                Summary = summary,
                Location = location,
                Description = description,
                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = DateTime.Parse(start),
                    TimeZone = "America/Denver",
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = DateTime.Parse(end),
                    TimeZone = "America/Denver",
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = true
                }
            };
            return gEvent;
        }
    }
}
