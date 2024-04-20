using Azure.Identity;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;

namespace Utility.GoogleCalendar
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        private const string CalendarEnvironment = "primary"; //Used to change the calendar environment between the main and testing environments. 
        public GoogleCalendarService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Gets a user's credential for the calendar, creates a connection to the google calendar service, and loads the event into storage.
        //Then, the storage is committed to the calendar. The id of the newly created event is stored for potential later use.
        public async Task<string> AddEvent(Event request, string userId, CancellationToken cancellationToken)
        {
            TokenRequest tokenRequest = new TokenRequest();
            var flow = await ValidateUser.ValidateUserCalendar(userId, _configuration);
            if (!ValidateUser.IsUserValidated(flow))                                    
            {   
                return String.Empty;
            }

            UserCredential? credential = null;
            try
            {
                tokenRequest.ClientSecret = flow.ClientSecrets.ClientSecret;
                tokenRequest.ClientId = flow.ClientSecrets.ClientId;
                tokenRequest.GrantType = "refresh_token";
                tokenRequest.Scope = CalendarService.ScopeConstants.Calendar;
                var token = await tokenRequest.ExecuteAsync(flow.HttpClient, flow.TokenServerUrl, cancellationToken, flow.Clock);
                credential = new UserCredential(flow, userId, token);
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
            catch
            {
                Console.WriteLine("x");
            }
            return String.Empty;
        }

        //Gets a user's credential for the calendar and creates a connection to the service. Checks to see if a google calendar event
        //With the passed in Id exists. If it exists, and has not been canceled, then it deletes the event. The string is returned in case someone wants to use it.
        public async Task<string> DeleteEvent(string eventId, string userId, CancellationToken cancellationToken)
        {
            var flow = await ValidateUser.ValidateUserCalendar(userId, _configuration);
            if (!ValidateUser.IsUserValidated(flow))
            {
                return String.Empty;
            }

            UserCredential? credential = null;
            try
            {
                credential = new UserCredential(flow, userId, await flow.FetchTokenAsync(userId, new TokenRequest(), CancellationToken.None));
            }
            catch
            {
                Console.WriteLine("x");
            }

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
