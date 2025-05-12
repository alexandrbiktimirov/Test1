namespace Template_2.Exceptions;

public class ValidationFailedException : Exception
{
    public ValidationFailedException(string? message) : base(message)
    {
    }
}