namespace Template_2.Exceptions;

public class VisitDoesNotExistException : Exception
{
    public VisitDoesNotExistException(string? message) : base(message)
    {
    }
}