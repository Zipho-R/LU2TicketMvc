namespace RaceDay.Api.Filters;

public static class SessionExtensions
{
    public static int GetCurrentUserId(this HttpContext context)
    {
        return context.Session.GetInt32(SessionKeys.UserId)
            ?? throw new InvalidOperationException("No user is logged in.");
    }

    public static string GetCurrentRole(this HttpContext context)
    {
        return context.Session.GetString(SessionKeys.Role)
            ?? throw new InvalidOperationException("No user is logged in.");
    }
}
