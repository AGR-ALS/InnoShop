namespace UserService.Application.Exceptions;

public class DbCreatingException(string error) : Exception(error);