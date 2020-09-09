namespace HideezMiddleware
{
    public enum ChangeServerAddressResult
    {
        Success,
        ConnectionTimedOut,
        KeyNotFound,
        UnauthorizedAccess,
        SecurityError,
        UnknownError,
    }
}
