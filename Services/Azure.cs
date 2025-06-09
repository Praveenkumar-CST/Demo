using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

namespace WiseHR.Frontend.Services
{
    public class TokenAuthenticationProvider : IAuthenticationProvider
    {
        private readonly string _accessToken;

        public TokenAuthenticationProvider(string accessToken)
        {
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }

        public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            request.Headers.Add("Authorization", $"Bearer {_accessToken}");
            return Task.CompletedTask;
        }
    }
}