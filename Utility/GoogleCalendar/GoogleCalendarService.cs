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

namespace Utility.GoogleCalendar
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        public GoogleCalendarService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateEvent(Event request, CancellationToken cancellationToken)
        {
            var settings = _configuration.GetSection("GoogleCalendarSettings");
            string[] scope = new string[] { settings["Scope"] };
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
                {
                    ClientId = settings["ClientId"],
                    ClientSecret = settings["ClientSecret"],
                },
                scope,
                settings["User"],
                cancellationToken);
            var services = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = settings["ApplicationName"],
            });

            var eventRequest = services.Events.Insert(request, settings["CalendarId"]);
            var requestCreate = await eventRequest.ExecuteAsync(cancellationToken);

            var id = requestCreate.Id;
            return id;
        }

        public async Task<string> DeleteEvent(string eventId, CancellationToken cancellationToken)
        {
            var settings = _configuration.GetSection("GoogleCalendarSettings");
            string[] scope = new string[] { settings["Scope"] };
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
            {
                ClientId = settings["ClientId"],
                ClientSecret = settings["ClientSecret"],
            },
                scope,
                settings["User"],
                cancellationToken);
            var services = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = settings["ApplicationName"],
            });

            var eventRequest = services.Events.Delete(settings["CalendarId"], eventId);
            var requestCreate = await eventRequest.ExecuteAsync(cancellationToken);

            return requestCreate;
        }
    }
}
