namespace SchoolLibrary.Application.Common.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message)
        : base(message)
        {
        }

        public ValidationException(
            string message,
            IDictionary<string, string[]> errors)
            : base(message)
        {
            Errors = errors;
        }

        public IDictionary<string, string[]> Errors { get; }
            = new Dictionary<string, string[]>();
    }
}
