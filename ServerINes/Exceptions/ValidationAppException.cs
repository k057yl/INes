using FluentValidation.Results;

namespace INest.Exceptions
{
    public class ValidationAppException : Exception
    {
        public IEnumerable<ValidationFailure> Errors { get; }

        public ValidationAppException(IEnumerable<ValidationFailure> errors)
            : base("Validation failed")
        {
            Errors = errors;
        }
    }
}