namespace SmimeLifecycle.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

public sealed class InvalidLifecycleTransitionException : DomainException
{
    public InvalidLifecycleTransitionException(string from, string to)
        : base($"Cannot transition certificate lifecycle from '{from}' to '{to}'.")
    {
    }
}

public sealed class SecretStorageException : DomainException
{
    public SecretStorageException()
        : base("Private keys, PFX files, PFX passwords, and access tokens must not be stored in the application database.")
    {
    }
}
