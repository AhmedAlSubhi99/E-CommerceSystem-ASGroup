using Microsoft.AspNetCore.Http;

namespace E_CommerceSystem.Services
{
    public interface ICookieTokenWriter
    {
        void Write(HttpResponse response, string jwt, int minutes);
        void Clear(HttpResponse response);
    }

    public class CookieTokenWriter : ICookieTokenWriter
    {
        private const string CookieName = "jwt";

        public void Write(HttpResponse response, string jwt, int minutes)
        {
            response.Cookies.Append(CookieName, jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(minutes)
            });
        }

        public void Clear(HttpResponse response)
        {
            response.Cookies.Append(CookieName, string.Empty, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
