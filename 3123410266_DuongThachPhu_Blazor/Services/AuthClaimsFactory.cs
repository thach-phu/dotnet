namespace BlazorWeb.Services;

public static class AuthClaimsFactory
{
    public static bool IsSafeLocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl)
        && returnUrl.StartsWith('/')
        && !returnUrl.StartsWith("//")
        && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative);
}
