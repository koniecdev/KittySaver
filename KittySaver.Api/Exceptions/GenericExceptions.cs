namespace KittySaver.Api.Exceptions;

public class NotFoundException(string objectName, string id) : Exception($"Could not found {objectName} with identifier: {id}");

public class BadRequestException(string message) : Exception(message);