namespace Template_2.Exceptions;

public class ClientDoesNotExistException : Exception
{
    public ClientDoesNotExistException(string? message) : base(message)
    {
    }
}