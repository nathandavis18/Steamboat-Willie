using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3.Data;

namespace Utility.GoogleCalendar
{
    public interface IGoogleCalendarService
    {
        public Task<string> AddEvent(Event request, string userId, CancellationToken cancellationToken);
        public Task<string> AddEvent(Event request, string userId, UserCredential cred, CancellationToken cancellationToken);
        public Task<string> DeleteEvent(string eventId, string userId, CancellationToken cancellationToken);
    }
}
