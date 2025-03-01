namespace UnixLauncher.Core.Exceptions
{
    class ValidatorException(string? message, int Value) : Exception(message)
    {
        public readonly int? value = Value;
    }
}
