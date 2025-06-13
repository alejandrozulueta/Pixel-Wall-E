using Core.Models;

namespace Core.Exceptions
{
    public class ExceptionWL : Exception
    {
        public Location Location { get; set; }

        public ExceptionWL(string? message, Location location)
            : base(message) => Location = location;

        public ExceptionWL(string? message, Exception? innerException, Location location)
            : base(message, innerException) => Location = location;
    }
}
