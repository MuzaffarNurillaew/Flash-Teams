namespace FlashTeams.Shared.Exceptions;

public class NotFoundBaseException : Exception
{
    public const int Code = 404;

    public NotFoundBaseException(Type type, Guid id)
    {
        Message = $"{type.Name} is not found with ID = '{id}'";
    }

    public NotFoundBaseException(Type type)
    {
        Message = $"{type.Name} is not found.";
    }

    public NotFoundBaseException(Type type, string reason)
    {
        Message = $"{type.Name} is not found: {reason}";
    }

    public override string Message { get; }
}
