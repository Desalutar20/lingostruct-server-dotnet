using src.Modules.Auth.OAuth.Providers;
using src.Modules.Users.Domain;

namespace src.Modules.Auth.OAuth;

internal sealed class OAuthClientFactory(IServiceProvider sp)
{
    public IOAuthClient Get(OAuthProvider provider) =>
        sp.GetRequiredKeyedService<IOAuthClient>(provider);
}
