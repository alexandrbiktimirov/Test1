namespace Template_2.Exceptions;

public class ServiceDoesNotExistException : Exception
{
    public ServiceDoesNotExistException(string? message) : base(message)
    {
    }
}