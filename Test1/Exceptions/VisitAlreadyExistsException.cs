namespace Template_2.Exceptions;

public class VisitAlreadyExistsException : Exception
{
    public VisitAlreadyExistsException(string? message) : base(message)
    {
    }
}