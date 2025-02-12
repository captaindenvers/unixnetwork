using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core
{
    internal class DataValidator : IDataValidator
    {
        public bool Validate(string text, ValidatorSettings validatorSettings)
        {
            if (string.IsNullOrEmpty(text)) return false;
            if (text.All(c => validatorSettings.AllowedSymbols.Contains(c))) return false;

            if(text.Length > validatorSettings.MaxLenght) return false;
            if(text.Length < validatorSettings.MinLenght) return false;

            return true;
        }
    }
}
