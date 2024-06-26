﻿using Azure.Identity;
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
        private const string CalendarEnvironment = "primary"; //Used to change the calendar environment between the main and testing environments. 

        public async Task<string> AddEvent(Event request, string userId, UserCredential cred)
        {
            try
            {
                var services = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = cred,
                    ApplicationName = "Steamboat Willie Scheduler",
                });

                var eventRequest = services.Events.Insert(request, CalendarEnvironment);
                var requestCreate = await eventRequest.ExecuteAsync(CancellationToken.None);

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
        public async Task<string> DeleteEvent(string eventId, string userId, UserCredential cred)
        {
            var services = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = cred,
                ApplicationName = "Steamboat Willie Scheduler",
            });

            var eventExists = await services.Events.Get(CalendarEnvironment, eventId).ExecuteAsync();

            if(eventExists.Status.ToUpper().Equals("cancelled".ToUpper()))
            {
                return String.Empty;
            }

            var eventRequest = services.Events.Delete(CalendarEnvironment, eventId);
            var requestDelete = await eventRequest.ExecuteAsync(CancellationToken.None);

            return requestDelete;
        }
    }
}
