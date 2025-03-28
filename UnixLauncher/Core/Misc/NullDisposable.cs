using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Misc
{
    /// <summary>
    /// Объект-пустышка, реализующий <see cref="IDisposable"/>
    /// </summary>
    public sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
