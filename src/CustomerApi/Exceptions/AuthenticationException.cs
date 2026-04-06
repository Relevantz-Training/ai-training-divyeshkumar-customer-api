namespace CustomerApi.Exceptions;

public sealed class AuthenticationException(string message) : Exception(message)
{
}
