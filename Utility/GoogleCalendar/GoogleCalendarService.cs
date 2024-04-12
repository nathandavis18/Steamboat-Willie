using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Utility.GoogleCalendar
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        private const string CalendarEnvironment = "primary";
        public GoogleCalendarService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateEvent(Event request, string userId, CancellationToken cancellationToken)
        {
            var settings = _configuration.GetSection("Authentication:Google");
            string[] scope = new string[] { "https://www.googleapis.com/auth/calendar" };
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
            {
                ClientId = settings["ClientId"],
                ClientSecret = settings["ClientSecret"],
            },
                scope,
                userId,
                cancellationToken);
            var services = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Steamboat Willie Scheduler",
            });

            var eventRequest = services.Events.Insert(request, CalendarEnvironment);
            var requestCreate = await eventRequest.ExecuteAsync(cancellationToken);

            var id = requestCreate.Id;
            return id;
        }

        public async Task<string> DeleteEvent(string eventId, string userId, CancellationToken cancellationToken)
        {
            var settings = _configuration.GetSection("Authentication:Google");
            string[] scope = new string[] { "https://www.googleapis.com/auth/calendar" };
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
            {
                ClientId = settings["ClientId"],
                ClientSecret = settings["ClientSecret"],
            },
                scope,
                userId,
                cancellationToken);
            var services = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Steamboat Willie Scheduler",
            });

            var eventExists = await services.Events.Get(CalendarEnvironment, eventId).ExecuteAsync();

            if(eventExists.Status.ToUpper().Equals("cancelled".ToUpper()))
            {
                return String.Empty;
            }

            var eventRequest = services.Events.Delete(CalendarEnvironment, eventId);
            var requestDelete = await eventRequest.ExecuteAsync(cancellationToken);

            return requestDelete;
        }
    }
}
