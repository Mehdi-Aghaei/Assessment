namespace GitHub.Api.Exceptions;

public class SecurityException(int statusCode, string errorMessage) : ServiceException(statusCode, errorMessage);