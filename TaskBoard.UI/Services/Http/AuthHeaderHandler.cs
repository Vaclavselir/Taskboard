using System.Net.Http.Headers;
using TaskBoard.UI.Services.Auth;

namespace TaskBoard.UI.Services.Http;

public class AuthHeaderHandler : DelegatingHandler
{

    private readonly TokenStore _tokenStore;

    public AuthHeaderHandler(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _tokenStore.GetToken();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }

}
