using DataAccess;
using Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Policy;

namespace SteamboatWillieWeb.ApiStuff
{
    public class AddGoogleAccount : PageModel
    {
        public static GoogleToken CreateToken(string tokenValue, string tokenName, string userId, UnitOfWork unitOfWork)
        {
            GoogleToken token = new GoogleToken()
            {
                UserId = userId,
                TokenName = tokenName,
                TokenValue = tokenValue
            };
            unitOfWork.GoogleToken.Add(token);
            unitOfWork.Commit();
            return token;
        }

        public static bool HasToken(string userId, string tokenName, UnitOfWork unitOfWork)
        {
            var token = unitOfWork.GoogleToken.Get(x => x.UserId.Equals(userId) && x.TokenName.Equals(tokenName));
            return token != null;
        }


    }
}
