using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Logger
{
    /// <summary>
    /// Представляет объект, управляющий областью (scope) логирования, который автоматически удаляет свою запись из коллекции при завершении работы.
    /// </summary>
    /// <remarks>
    /// Класс <c>ScopeDisposable</c> используется для создания временной области логирования, которая регистрируется в общем списке областей.
    /// При вызове метода <see cref="Dispose()"/> происходит попытка удаления идентификатора области (<c>_scope</c>) из переданного списка областей (<c>_scopes</c>), что позволяет корректно завершить контекст логирования.
    /// 
    /// Обратите внимание, что данный класс следует использовать в конструкции <c>using</c> для гарантированного освобождения ресурсов и своевременного завершения области логирования.
    /// </remarks>
    public class ScopeDisposable : IDisposable
    {
        private readonly string _scope;
        private readonly List<string> _scopes;
        private bool _disposed;

        public ScopeDisposable(string scope, List<string> scopes)
        {
            _scope = scope;
            _scopes = scopes;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_scopes.Contains(_scope) && _disposed)
            {
                if (disposing)
                {
                    // Удаляем текущий скоп только при явном вызове Dispose
                    _scopes.Remove(_scope);
                }

                _disposed = true;
            }
        }
    }
}
