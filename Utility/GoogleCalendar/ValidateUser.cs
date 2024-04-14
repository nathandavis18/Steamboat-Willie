using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace Utility.GoogleCalendar
{
    public class ValidateUser
    {
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

        public static bool IsUserValidated(UserCredential credential)
        {
            return credential.Token.AccessToken != null;
        }
    }
}
