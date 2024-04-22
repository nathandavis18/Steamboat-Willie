using Google.Apis.Calendar.v3.Data;

namespace Utility.GoogleCalendar
{
    public class EventCreater
    {
        public static Event CreateEvent(string summary, string location, string description, DateTime start, DateTime end)
        {
            Event gEvent = new Event()
            {
                Summary = summary,
                Location = location,
                Description = description,
                Start = new EventDateTime()
                {
                    DateTimeDateTimeOffset = start
                },
                End = new EventDateTime()
                {
                    DateTimeDateTimeOffset = end,
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = true
                },
                
            };
            return gEvent;
        }
    }
}
