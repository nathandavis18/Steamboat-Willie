using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.GoogleCalendar
{
    public interface IGoogleCalendarSettings
    {
        string ClientId {  get; }
        string ClientSecret { get; }
        List<string> Scope {  get; }
        string ApplicationName { get; }
        string User {  get; }
        string CalendarId { get; }
    }
}
