using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class GramaticalExceptions : ExceptionWL
    {
        public GramaticalExceptions(string? message, Location location) : base(message, location){ }

        public GramaticalExceptions(string? message, Exception? innerException, Location location)
    :       base(message, innerException, location) { }
    }
}
