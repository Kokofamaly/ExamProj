namespace WordCardsApi.CustomExceptions;

public class NotEnoughWordsException : Exception
{
    public NotEnoughWordsException(int currentNumberOfWords) 
    : base($"You must have atleast 100 words to create learning session. Current: {currentNumberOfWords} words")
    {
        
    }
}