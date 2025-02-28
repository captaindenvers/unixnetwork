using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Exceptions
{
    class ValidatorException(string? message, int Value) : Exception(message)
    {
        public readonly int? value = Value;
    }
}
