namespace FlashTeams.Shared.Exceptions;

public class NotFoundException<T> : NotFoundBaseException
    where T : class
{
    public NotFoundException()
        : base(typeof(T))
    {
        Message = $"{typeof(T).Name} is not found.";
    }

    public override string Message { get; }
}