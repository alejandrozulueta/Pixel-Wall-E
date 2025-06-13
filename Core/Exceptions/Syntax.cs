using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class SyntaxException : ExceptionWL
    {
        public SyntaxException(string? message, Location location)
    : base(message, location) { }

        public SyntaxException(string? message, Exception? innerException, Location location)
            : base(message, innerException, location) { }
    }
}
