namespace Template_2.Exceptions;

public class MechanicDoesNotExistException : Exception
{
    public MechanicDoesNotExistException(string? message) : base(message)
    {
    }
}