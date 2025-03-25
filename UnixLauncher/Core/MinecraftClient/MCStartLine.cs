using System.Text;

namespace UnixLauncher.Core.MinecraftClient
{
    class MCStartLine
    {
        private readonly StringBuilder stringBuilder;

        public MCStartLine()
        {
            stringBuilder = new StringBuilder();
            stringBuilder.Append("java ");
            
        }

        /// <summary>
        /// Добавляет аргумент в stringBuilder.
        /// В переменной after пробел автоматически НЕ ставится.
        /// </summary>
        public void AddArgument(string before, object? arg, string after)
        {
            stringBuilder.Append(before);
            stringBuilder.Append(arg);
            stringBuilder.Append(after);
        }

        public override string ToString()
        {
            return stringBuilder.ToString();
        }
    }
}
