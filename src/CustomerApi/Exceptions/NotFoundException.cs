namespace CustomerApi.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
}
