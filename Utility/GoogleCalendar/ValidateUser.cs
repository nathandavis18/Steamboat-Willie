using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace Utility.GoogleCalendar
{
    public class ValidateUser
    {
        //This method compares the userId to the current calendar config. If a user with this id exists, then it retrieves that user's credentials.
        //If no such user exists, it creates a new credential for that user, once they link their google account and accept the terms.
        public static async Task<UserCredential> ValidateUserCalendar(string userId, IConfiguration configuration)
        {
            var settings = configuration.GetSection("Authentication:Google");
            string[] scope = new string[] { "https://www.googleapis.com/auth/calendar" };
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets()
                {
                    ClientId = settings["ClientId"],
                    ClientSecret = settings["ClientSecret"],
                },
                scope,
                userId,
                new CancellationToken(false));

            return credential;
        }

        //If the access token is null, the user has not been validated, and processes requiring the credential should not continue.
        public static bool IsUserValidated(UserCredential credential)
        {
            return credential.Token.AccessToken != null;
        }
    }
}
