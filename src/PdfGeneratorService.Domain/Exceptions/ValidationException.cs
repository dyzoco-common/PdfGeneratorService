namespace PdfGeneratorService.Domain.Exceptions;

public class ValidationException : DomainException
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public ValidationException(string message, IEnumerable<ValidationError> errors)
        : base(message)
    {
        Errors = errors.ToList().AsReadOnly();
    }

    public ValidationException(string field, string message)
        : base(message)
    {
        Errors = new List<ValidationError> { new(field, message) }.AsReadOnly();
    }
}
