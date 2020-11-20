using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorOidcDemo.Pages
{
    public class _HostAuthModel : PageModel
    {
        private readonly BlazorServerAuthStateCache _cache;

        public _HostAuthModel(BlazorServerAuthStateCache cache)
        {
            _cache = cache;
        }

        public async Task<IActionResult> OnGet()
        {
            if(User.Identity.IsAuthenticated)
            {
                var sid = User.Claims.Where(c => c.Type.Equals("sid")).Select(c => c.Value).FirstOrDefault();

                if(sid != null && !_cache.HasSubjectId(sid))
                {
                    var authResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
                    var expiration = authResult.Properties.ExpiresUtc.Value;
                    var accessToken = await HttpContext.GetTokenAsync("access_token");
                    var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
                    _cache.Add(sid, expiration, accessToken, refreshToken);
                }
            }
            return Page();
        }

        public IActionResult OnGetLogin()
        {
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(5),
                RedirectUri = Url.Content("~/")
            };

            return Challenge(authProps, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public async Task OnGetLogout()
        {
            var authProps = new AuthenticationProperties
            {
                RedirectUri = Url.Content("~/")
            };
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, authProps);
        }
    }
}
