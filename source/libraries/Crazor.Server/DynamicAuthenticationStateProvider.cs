using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Crazor.Server
{
    public class DynamicAuthenticationStateProvider : AuthenticationStateProvider, IHostEnvironmentAuthenticationStateProvider
    {
        private Task<AuthenticationState> _state = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        public DynamicAuthenticationStateProvider()
        {
        }


        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return _state;
        }

        public void SetAuthenticationState(Task<AuthenticationState> authenticationStateTask)
        {
            _state = authenticationStateTask;
            NotifyAuthenticationStateChanged(_state);
        }
    }
}
