using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace Utility.GoogleCalendar
{
    public class ValidateUser
    {
        //This method compares the userId to the current calendar config. If a user with this id exists, then it retrieves that user's credentials.
        //If no such user exists, it creates a new credential for that user, once they link their google account and accept the terms.

        //If the access token is null, the user has not been validated, and processes requiring the credential should not continue.
        public static bool IsUserValidated(GoogleAuthorizationCodeFlow? flow)
        {
            return flow != null;
        }
    }
}
