using System.Net.Http.Headers;
using TaskBoard.UI.Services.Auth;

namespace TaskBoard.UI.Services.Http;

public class AuthHeaderHandler : DelegatingHandler
{

    private readonly TokenStore _store;

    public AuthHeaderHandler(TokenStore store)
    {

        _store = store;

    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        var token = _store.Token;

        if (!string.IsNullOrWhiteSpace(token))
        {

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        }

        return base.SendAsync(request, cancellationToken);

    }

}
