namespace WordCardsApi.CustomExceptions;

public class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email) 
    : base($"Email '{email}' is already taken.")
    {
        
    }
}