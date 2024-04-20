using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class UserCredentials
    {
        private readonly UnitOfWork _unitOfWork;
        public UserCredentials(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UserCredential?> GetUser(string userId, IConfiguration configuration)
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
                        Scopes = new[] { CalendarService.Scope.Calendar },
                        ProjectId = userId,
                    });
                }

                UserCredential? cred = null;
                var refreshToken = _unitOfWork.GoogleToken.Get(x => x.UserId.Equals(userId) && x.TokenName.Equals("refresh_token")).TokenValue;
                var token = await flow.RefreshTokenAsync(userId, refreshToken, CancellationToken.None);
                cred = new UserCredential(flow, userId, token);
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
