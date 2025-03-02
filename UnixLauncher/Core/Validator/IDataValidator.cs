namespace UnixLauncher.Core.Validator
{
    internal interface IDataValidator
    {
        bool Validate(string text, ValidatorSettings validatorSettings);
    }
}
