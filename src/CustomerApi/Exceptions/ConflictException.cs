namespace CustomerApi.Exceptions;

public sealed class ConflictException(string message) : Exception(message)
{
}
