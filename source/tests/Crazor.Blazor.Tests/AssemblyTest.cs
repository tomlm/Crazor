using Crazor.Blazor.Tests.Cards.CodeOnlyView;
using Crazor.Server;
using Crazor.Test;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;

namespace Crazor.Blazor.Tests
{
    [TestClass]
    public class AssemblyTest : CardTest
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            InitCardServices((services) =>
            {
                services.AddCrazor();
                services.AddCrazorServer();
                services.AddCrazorBlazor();
                services.AddAuthentication();
                services.AddAuthorization();
                services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>((sp) =>
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, "John"),
                        new Claim(ClaimTypes.NameIdentifier, "johnd@test.com"),
                        new Claim("name", "John Doe"),
                    };
                    var identity = new ClaimsIdentity(claims, "Test");
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    var authProvider = new ServerAuthenticationStateProvider();
                    authProvider.SetAuthenticationState(Task.FromResult(new AuthenticationState(claimsPrincipal)));
                    return authProvider;
                });

                // add your own dependencies here...
                // services.Add<IFoo>();
            });
        }
    }
}
