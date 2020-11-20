using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorOidcDemo
{
    public class BlazorServerAuthState : RevalidatingServerAuthenticationStateProvider
    {
        private readonly BlazorServerAuthStateCache _cache;

        public BlazorServerAuthState(ILoggerFactory loggerFactory, BlazorServerAuthStateCache cache) : base(loggerFactory)
        {
            _cache = cache;
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(10); //TODO: read from config option

        protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            var sid = authenticationState.User.Claims.Where(c => c.Type.Equals("sid")).Select(c => c.Value).FirstOrDefault();

            if(sid != null && _cache.HasSubjectId(sid))
            {
                var authData = _cache.Get(sid);
                if(DateTimeOffset.UtcNow >= authData.Expiration)
                {
                    _cache.Remove(sid);
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(true);
        }
    }
}
