namespace UnixLauncher.Core.Exceptions
{
    class ValidatorException(string? message, int value) : Exception(message)
    {
        public readonly int? Value = value;
    }
}
