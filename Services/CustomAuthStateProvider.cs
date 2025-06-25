using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace WiseHR.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                var role = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "userRole");
                var identity = new ClaimsIdentity();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                    if (jsonToken != null)
                    {
                        var claims = jsonToken.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();
                        if (!string.IsNullOrWhiteSpace(role))
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }
                        identity = new ClaimsIdentity(claims, "jwt");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(role))
                {
                    var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
                    identity = new ClaimsIdentity(claims, "session");
                }

                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyAuthenticationStateChanged()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}