using System.Net;

namespace FlashTeams.Shared.Exceptions;

public class FlashTeamsException : Exception
{
    public FlashTeamsException(HttpStatusCode code, string message)
        : base(message)
    {
        Code = (int)code;
    }

    public FlashTeamsException(int code, string message)
        : base(message)
    {
        Code = code;
    }

    public int Code { get; set; }
}