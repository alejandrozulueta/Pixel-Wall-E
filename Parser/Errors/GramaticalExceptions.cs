using Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Errors
{
    public class GramaticalExceptions : Exception
    {
        Location? Location { get; set; }

        public GramaticalExceptions(string? message, Location? location = null) 
            : base(message) => Location = location;

        public GramaticalExceptions(string? message, Exception? innerException, Location? location = null)
            : base(message, innerException) => Location = location;
    }
}
