using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core
{
    internal interface IDataValidator
    {
        bool Validate(string text, ValidatorSettings validatorSettings);
    }
}
