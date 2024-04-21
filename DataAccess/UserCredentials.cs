using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public class UserCredentials
    {
        private readonly UnitOfWork _unitOfWork;
        public UserCredentials(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserCredential?> GetUserAsync(string userId, IConfiguration configuration)
        {
            GoogleAuthorizationCodeFlow? flow = null;
            try
            {
                var settings = configuration.GetSection("Authentication:Google");
                if (settings.Exists())
                {
                    flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = settings.GetValue(typeof(string), "ClientId") as string,
                            ClientSecret = settings.GetValue(typeof(string), "ClientSecret") as string,
                        },
                        ProjectId = userId,
                        IncludeGrantedScopes = true
                    });
                }
                UserCredential? cred = null;
                if (flow != null)
                {
                    var gt = _unitOfWork.GoogleToken.Get(x => x.UserId.Equals(userId) && x.TokenName.Equals("refresh_token"));
                    if (gt != null)
                    {
                        var refreshToken = gt.TokenValue;
                        var token = await flow.RefreshTokenAsync(userId, refreshToken, CancellationToken.None);
                        cred = new UserCredential(flow, userId, token);
                    }
                }
                return cred;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
    }
}
