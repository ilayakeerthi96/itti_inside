namespace Itti.Inside.Web.Models;

public sealed record ErrorViewModel(int StatusCode, string Title, string Message)
{
    public static ErrorViewModel ForStatus(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => new(statusCode, "That didn't work", "Your session may have expired. Go back, refresh the page and try again."),
        StatusCodes.Status403Forbidden => new(statusCode, "Access denied", "You don't have permission to view this page."),
        StatusCodes.Status404NotFound => new(statusCode, "Page not found", "The page you're looking for doesn't exist or has moved."),
        StatusCodes.Status429TooManyRequests => new(statusCode, "Too many attempts", "Please wait a minute before trying again."),
        _ => new(statusCode, "Something went wrong", "We couldn't complete your request. Please try again in a few minutes."),
    };
}
