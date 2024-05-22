using Crazor.Server;
using Crazor.Test;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Crazor.Tests
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
                services.AddSingleton<AuthenticationStateProvider, FakeAuthenticationStateProvider>();
                services.AddSingleton<IAuthorizationService, FakeAuthorizationService>();

                //services.AddMvc()
                //    // .AddRazorOptions((options) => { var x = options; })
                //    // .AddRazorPagesOptions((options) => { var y = options; })
                //    .AddRazorRuntimeCompilation();
                // add your own dependencies here...
                // services.Add<IFoo>();
            });
        }
    }

    public class FakeAuthenticationStateProvider : AuthenticationStateProvider
    {
        public FakeAuthenticationStateProvider()
        {
        }

        // This static method isn't really necessary. You could call the 
        // constructor directly. I just like how it makes it more clear
        // what the fake is doing within the test.
        public static FakeAuthenticationStateProvider ForPrincipal(ClaimsPrincipal principal)
        {
            return new FakeAuthenticationStateProvider();
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(null));
        }
    }

    public class FakeAuthorizationService : IAuthorizationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            throw new NotImplementedException();
        }
    }

}
